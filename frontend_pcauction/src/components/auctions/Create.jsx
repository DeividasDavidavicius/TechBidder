import { useNavigate } from "react-router-dom";
import { useUser } from "../../contexts/UserContext";
import { useContext, useEffect, useState } from "react";
import SnackbarContext from "../../contexts/SnackbarContext";
import { Box, Button, Container, CssBaseline,  FormControl, FormHelperText, Grid, Input, InputLabel, MenuItem, Select, TextField, Typography } from "@mui/material";
import { isDatePastNow, isEndDateLater, isValidDescription, isValidMinInc, isValidTitle } from "./Validations";
import { DateTimePicker, LocalizationProvider } from "@mui/x-date-pickers";
import { AdapterDayjs } from '@mui/x-date-pickers/AdapterDayjs';
import { useTheme } from '@mui/material/styles';
import 'dayjs/locale/lt';
import { checkTokenValidity, refreshAccessToken } from "../../services/AuthService";
import PATHS from "../../utils/Paths";
import { postAuction } from "../../services/AuctionService";

function CreateAuction() {
    const navigate = useNavigate();
    const theme = useTheme();
    const { role, setLogin, setLogout } = useUser();
    const openSnackbar = useContext(SnackbarContext);

    const [condition, setCondition] = useState("New");
    const [selectedImage, setSelectedImage] = useState(null);
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

        reader.onload = () => {
          setSelectedImage(reader.result);
        };

        if (file) {
          reader.readAsDataURL(file);
        }
      };

    const handleSubmit = async (e) => {
        e.preventDefault();

        const title = e.target.title.value;
        const description = e.target.description.value;
        const startDate = e.target.startDate.value;
        const endDate = e.target.endDate.value;
        const minInc = e.target.minInc.value;
        const manufacturer = e.target.manufacturer.value ? e.target.manufacturer.value : "";

        let errors = [];
        setValidationErrors(errors);
        if (!isValidTitle(title)) errors.title = "Title must be at least 5 characters long";
        if (!isValidDescription(description)) errors.description = "Description must be at least 10 characters long";
        if(!isDatePastNow(startDate)) errors.startDate = "Start date must be later than current time";
        if(!isDatePastNow(endDate)) errors.endDate = "End date must be later than current time";
        if(!isEndDateLater(startDate, endDate)) errors.endDate = "End date must be later than start date";
        if(!isValidMinInc(minInc)) errors.minInc = "Minimum increment must be set";
        if(startDate == null || startDate === "") errors.startDate = "Auction start date must be set";
        if(endDate == null || endDate === "") errors.endDate = "Auction end date must be set";
        if(condition === null || condition === "") errors.condition = "Condition must be set"
        if(selectedImage === null || selectedImage === "") errors.image = "Image must be selected"

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

        const startDateLocal = new Date(startDate);
        const isoStartDate = startDateLocal.toISOString()
        const endDateLocal = new Date(endDate);
        const isoEndDate = endDateLocal.toISOString();

        const picture = "a";
        const postData = {name: title, description, startDate: isoStartDate, endDate: isoEndDate, minIncrement: minInc, condition, manufacturer, picture};

        try {
            await postAuction(postData);
            navigate(PATHS.MAIN); // TODO: Pakeisti i listo view'a  o  ne main page
            openSnackbar('Auction created successfully!', 'success');
        } catch(error) {
            openSnackbar(error.response.data.errorMessage, "error")
        }
    };

    useEffect(() => {
        if (!role.includes("RegisteredUser")) {
            openSnackbar('You can not create auction!', 'error');
            navigate('/');
        }
    });

    return (
        <Container component="main" maxWidth="md">
            <CssBaseline />
            <Box
                sx={{
                    marginTop: 8,
                    display: 'flex',
                    flexDirection: 'column',
                    alignItems: 'center',
                }}
            >
                <Typography component="h1" variant="h5">
                    Create new auction
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
                                autoFocus
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
                            />
                        </Grid>
                        <Grid item xs={12}>
                            <LocalizationProvider dateAdapter={AdapterDayjs} adapterLocale="lt">
                                <DateTimePicker
                                    label="Auction start date *"
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
                        <Grid item xs={12}>
                            <LocalizationProvider dateAdapter={AdapterDayjs} adapterLocale="lt">
                            <DateTimePicker
                                label="Auction end date *"
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
                                inputProps={{ min: 0 }}
                                defaultValue={0}
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
                            />
                        </Grid>
                        {selectedImage && (
                            <Grid item xs={12}>
                                <img src={selectedImage} alt="Selected" style={{ maxWidth: '50%' }} />
                            </Grid>
                        )}
                        <Grid item xs={12}>
                            <Input
                                type="file"
                                id="image-input"
                                accept="image/*"
                                onChange={handleImageChange}
                                style={{ display: 'none' }}
                            />
                            <label htmlFor="image-input">
                                <Button variant="contained" component="span" fullWidth sx={{ mt: 3, mb: 2, bgcolor: '#138c94' }}
>
                                Select Image
                                </Button>
                                {validationErrors.image && <FormHelperText sx={{ color: theme.palette.error.main}}>{validationErrors.image}</FormHelperText>}
                            </label>
                        </Grid>
                    </Grid>
                    <Button
                        type="submit"
                        fullWidth
                        variant="contained"
                        sx={{ mt: 3, mb: 2, bgcolor: '#0d6267' }}
                    >
                        Create auction
                    </Button>
                </Box>
            </Box>
        </Container>
    );
}

export default CreateAuction;
