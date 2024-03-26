import { useNavigate, useParams } from "react-router-dom";
import { useUser } from "../../contexts/UserContext";
import { useContext, useEffect, useState } from "react";
import SnackbarContext from "../../contexts/SnackbarContext";
import { Box, Button, Container, CssBaseline,  FormControl, FormHelperText, Grid, Input, InputLabel, MenuItem, Select, TextField, Typography } from "@mui/material";
import { isDatePastNow, isEndDateLater, isValidDescription, isValidMinInc, isValidTitle } from "./Validations";
import { DateTimePicker, LocalizationProvider } from "@mui/x-date-pickers";
import { AdapterDayjs } from '@mui/x-date-pickers/AdapterDayjs';
import { useTheme } from '@mui/material/styles';
import 'dayjs/locale/lt';
import dayjs from 'dayjs';
import { checkTokenValidity, refreshAccessToken } from "../../services/AuthService";
import PATHS from "../../utils/Paths";
import { getAuction, putAuction } from "../../services/AuctionService";
import { getHighestBid } from "../../services/BIdService";

function EditAuction() {
    const navigate = useNavigate();
    const theme = useTheme();
    const { role, setLogin, setLogout, getUserId } = useUser();
    const openSnackbar = useContext(SnackbarContext);
    const [startingName, setStartingName] = useState("");
    const [name, setName] = useState("");
    const [description, setDescription] = useState("");
    const [startDateLocal, setStartDateLocal] = useState(null);
    const [endDateLocal, setEndDateLocal] = useState(null);
    const [manufacturer, setManufacturer] = useState("");
    const [minIncrement, setMinIncrement] = useState(0);
    const { auctionId } = useParams();
    const [condition, setCondition] = useState("New");
    const [selectedImage, setSelectedImage] = useState(null);
    const [imageType, setImageType] = useState(null);
    const [imageFile, setImageFile] = useState(undefined);
    const [imageUri, setImageUri] = useState(null);
    const [status, setStatus] = useState("");
    const [validationErrors, setValidationErrors] = useState({
        title: null,
        description: null,
        startDate: null,
        endDate: null,
        minInc: null,
        condition: null,
        image: null
    });

    const handleConditionChange = (e) => {
        setCondition(e.target.value);
    }

    const handleImageChange = (e) => {
        validationErrors.image = null;
        const file = e.target.files[0];
        const reader = new FileReader();

        setImageType(file && file.type ? file.type : null);

        reader.onload = () => {
          setSelectedImage(reader.result);
        };

        if (file) {
          reader.readAsDataURL(file);
        }

        setImageFile(file);
        setImageUri(URL.createObjectURL(file) || imageUri);
    };

    const handleNameChange = (e) => {
        setName(e.target.value);
    };

    const handleDescriptionChange = (e) => {
        setDescription(e.target.value);
    };

    const handleMinIncrementChange = (e) => {
        setMinIncrement(e.target.value);
    };

    const handleManufacturerChange = (e) => {
        setManufacturer(e.target.value);
    };

    const handleStartDateChange = (e) => {
        setStartDateLocal(e.$d.toLocaleString());
    };

    const handleEndDateChange = (e) => {
        setEndDateLocal(e.$d.toLocaleString());
    };

    const handleSubmit = async (e) => {
        e.preventDefault();

        const startDate = startDateLocal
        const endDate = endDateLocal;
        setManufacturer(manufacturer ? manufacturer : "");

        let errors = [];
        setValidationErrors(errors);
        if (!isValidTitle(name)) errors.title = "Title must be at 5 - 30 characters long";
        if (!isValidDescription(description)) errors.description = "Description must be at least 10 characters long";
        if(!isDatePastNow(startDate) && status === "New") errors.startDate = "Start date must be later than current time";
        if(!isDatePastNow(endDate)) errors.endDate = "End date must be later than current time";
        if(!isEndDateLater(startDate, endDate)) errors.endDate = "End date must be later than start date";
        if(!isValidMinInc(minIncrement)) errors.minInc = "Minimum increment must 0 or a positive number";
        if(startDate == null || startDate === "") errors.startDate = "Auction start date must be set";
        if(endDate == null || endDate === "") errors.endDate = "Auction end date must be set";
        if(condition === null || condition === "") errors.condition = "Condition must be set"
        if(!imageType || !imageType.startsWith('image/')) errors.image = "Please select a valid image file";

        if (Object.keys(errors).length > 0) {
            setValidationErrors(errors);
            return;
        }

        const accessToken = localStorage.getItem('accessToken');
        if (!checkTokenValidity(accessToken)) {
            const result = await refreshAccessToken();
            if (!result.success) {
                openSnackbar('You need to login!', 'error');
                setLogout();
                navigate(PATHS.LOGIN);
                return;
            }

            setLogin(result.response.data.accessToken, result.response.data.refreshToken);
        }

        const newStartDate = new Date(startDate);
        const isoStartDate = newStartDate.toISOString()
        const newEndDate = new Date(endDate);
        const isoEndDate = newEndDate.toISOString();

        const formData = new FormData();
        formData.append("name", name);
        formData.append("description", description);
        formData.append("startDate", isoStartDate);
        formData.append("endDate", isoEndDate);
        formData.append("minIncrement", minIncrement);
        formData.append("condition", condition);
        formData.append("manufacturer", manufacturer);
        formData.append("image", imageFile);

        try {
            await putAuction(formData, auctionId);
            navigate(PATHS.MAIN); // TODO: Pakeisti i listo view'a  o  ne main page
            openSnackbar('Auction updated successfully!', 'success');
        } catch(error) {
            openSnackbar(error.response.data.errorMessage, "error")
        }
    };

    useEffect(() => {
        const getAuctionData = async () => {
            try {
                const result = await getAuction(auctionId);
                const highestBid = await getHighestBid(auctionId);

                if (!(role.includes("RegisteredUser") && result.userId === getUserId()) && !role.includes("Admin")) {

                    openSnackbar('You can not edit this auction!', 'error');
                    navigate(PATHS.MAIN);
                }

                if(result.status !== "New" && highestBid.amount !== -1)
                {
                    openSnackbar('This auction can not be updated!', 'error');

                    navigate(PATHS.AUCTIONINFO.replace(":auctionId", auctionId));
                }

                console.log(result.status);

                setStatus(result.status);
                setImageType("image/");
                setStartingName(result.name);
                setName(result.name);
                setDescription(result.description);
                setMinIncrement(result.minIncrement);
                setCondition(result.condition);
                setManufacturer(result.manufacturer ? result.manufacturer : "");
                setImageUri(result.imageUri);

                const offsetInMilliseconds = new Date().getTimezoneOffset() * 60000;
                const utcDateStart = new Date(result.startDate);
                const utcDateEnd = new Date(result.endDate);

                const localDateStart = new Date(utcDateStart.getTime() - offsetInMilliseconds).toLocaleString();
                const localDateEnd = new Date(utcDateEnd.getTime() - offsetInMilliseconds).toLocaleString();

                setStartDateLocal(localDateStart.slice(0, -3));
                setEndDateLocal(localDateEnd.slice(0, -3));

            }
            catch {
                openSnackbar('This auction does not exist!', 'error');
                navigate(PATHS.MAIN);
            }
        };

        getAuctionData();
    }, [auctionId, getUserId, navigate, openSnackbar, role]);

    return (
        <Container component="main" maxWidth="sm">
            <CssBaseline />
            <Box
                sx={{
                    marginTop: 4,
                    display: 'flex',
                    flexDirection: 'column',
                    alignItems: 'center',
                }}
            >
                <Typography component="h1" variant="h5" style={{ fontWeight: 'bold' }}>
                    EDIT AUCTION '{startingName}'
                </Typography>


                <Box component="form" noValidate onSubmit={(event) => handleSubmit(event)} sx={{ mt: 3 }}>
                    <Grid container spacing={2}>
                        <Grid item xs={12}>
                            <TextField
                                error={Boolean(validationErrors.title)}
                                helperText={validationErrors.title}
                                required
                                fullWidth
                                id="title"
                                label="Title"
                                name="title"
                                value={name}
                                onChange={handleNameChange}
                            />
                        </Grid>
                        <Grid item xs={12}>
                            <TextField
                                error={Boolean(validationErrors.description)}
                                helperText={validationErrors.description}
                                required
                                fullWidth
                                id="description"
                                label="Description"
                                name="description"
                                multiline
                                rows={3}
                                value={description}
                                onChange={handleDescriptionChange}
                                />
                        </Grid>

                        {status === "New" ? (
                        <>
                        <Grid item xs={12}>
                            <LocalizationProvider dateAdapter={AdapterDayjs} adapterLocale="lt">
                                <DateTimePicker
                                    label="Auction start date *"
                                    value={dayjs(startDateLocal)}
                                    onChange={handleStartDateChange}
                                    sx={{ width: '100%' }}
                                    name="startDate"
                                    slotProps={{
                                        textField: {
                                          error: Boolean(validationErrors.startDate),
                                          helperText: validationErrors.startDate,
                                        },
                                      }}
                                />
                            </LocalizationProvider>
                        </Grid>
                        </> )
                        :
                        (
                            <>
                            <Grid item xs={12}>
                                <LocalizationProvider dateAdapter={AdapterDayjs} adapterLocale="lt">
                                    <DateTimePicker
                                        label="Auction start date *"
                                        value={dayjs(startDateLocal)}
                                        onChange={handleStartDateChange}
                                        sx={{ width: '100%' }}
                                        disabled
                                        name="startDate"
                                        slotProps={{
                                            textField: {
                                              error: Boolean(validationErrors.startDate),
                                              helperText: validationErrors.startDate,
                                            },
                                          }}
                                    />
                                </LocalizationProvider>
                            </Grid>
                            </> )
                        }
                        <Grid item xs={12}>
                            <LocalizationProvider dateAdapter={AdapterDayjs} adapterLocale="lt">
                                <DateTimePicker
                                    label="Auction end date *"
                                    value={dayjs(endDateLocal)}
                                    onChange={handleEndDateChange}
                                    sx={{ width: '100%' }}
                                    name="endDate"
                                    slotProps={{
                                        textField: {
                                        error: Boolean(validationErrors.endDate),
                                        helperText: validationErrors.endDate,
                                        },
                                    }}
                                />
                            </LocalizationProvider>
                        </Grid>
                        <Grid item xs={12}>
                            <TextField
                                error={Boolean(validationErrors.minInc)}
                                helperText={validationErrors.minInc}
                                required
                                fullWidth
                                id="minInc"
                                label="Minimum increment"
                                name="minInc"
                                type="number"
                                value = {minIncrement}
                                onChange = { handleMinIncrementChange }
                            />
                        </Grid>
                        <Grid item xs={12}>
                            <FormControl fullWidth>
                                <InputLabel id="condition-label" sx={{color: validationErrors.condition ? theme.palette.error.main : 'inherit', }}>Condition *</InputLabel>
                                    <Select
                                        labelId="condition-label"
                                        id="condition"
                                        label="Condition"
                                        value={condition}
                                        onChange={handleConditionChange}
                                        error={Boolean(validationErrors.condition)}
                                        required
                                        sx={{ textAlign: 'left' }}
                                    >
                                        <MenuItem value={"New"}>New</MenuItem>
                                        <MenuItem value={"Used"}>Used</MenuItem>
                                </Select>
                                {validationErrors.condition && <FormHelperText sx={{ color: theme.palette.error.main}}>{validationErrors.condition}</FormHelperText>}
                            </FormControl>
                        </Grid>
                        <Grid item xs={12}>
                            <TextField
                                fullWidth
                                id="manufacturer"
                                label="Manufacturer"
                                name="manufacturer"
                                value={manufacturer}
                                onChange = {handleManufacturerChange}
                            />
                        </Grid>

                        <Grid item xs={12} style={{ height: '30vh', display: 'flex', justifyContent: 'center', alignItems: 'center' }}>
                            {selectedImage ? (
                                <div style={{ width: '100%', height: '90%', display: 'flex', justifyContent: 'center', alignItems: 'center' }}>
                                    <img src={imageUri} alt="Selected" style={{ maxWidth: '100%', maxHeight: '100%', objectFit: 'contain' }} />
                                </div>
                            ) : (
                                <div style={{ width: '100%', height: '90%', display: 'flex', justifyContent: 'center', alignItems: 'center' }}>
                                    {<img src={imageUri} alt="OldPicture" style={{ maxWidth: '100%', maxHeight: '100%', objectFit: 'contain' }} />}
                                </div>
                            )}
                        </Grid>
                        <Grid item xs={12}>
                            <Input
                                type="file"
                                id="image-input"
                                accept="image/*"
                                onChange={handleImageChange}
                                style={{ display: 'none' }}
                            />
                            <label htmlFor="image-input">
                                <Button variant="contained" component="span" fullWidth sx={{ mt: 1, mb: 2, bgcolor: '#138c94', '&:hover': { backgroundColor: '#07383b'} }}>
                                    Select Image
                                </Button>
                                {validationErrors.image && <FormHelperText sx={{ color: theme.palette.error.main }}>{validationErrors.image}</FormHelperText>}
                            </label>
                        </Grid>
                    </Grid>
                    <Button
                        type="submit"
                        fullWidth
                        variant="contained"
                        sx={{ mt: 1, mb: 2, bgcolor: '#0d6267', '&:hover': { backgroundColor: '#07383b'} }}
                    >
                        Update auction
                    </Button>
                </Box>
            </Box>
        </Container>
    );
}

export default EditAuction;
