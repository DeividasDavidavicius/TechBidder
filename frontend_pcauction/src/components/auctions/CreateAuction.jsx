import { useNavigate } from "react-router-dom";
import { useUser } from "../../contexts/UserContext";
import { useContext, useEffect, useState } from "react";
import SnackbarContext from "../../contexts/SnackbarContext";
import { Autocomplete, Box, Button, Checkbox, Container, CssBaseline,  FormControl, FormControlLabel, FormHelperText, Grid, Input, InputLabel, MenuItem, Select, Skeleton, TextField, Typography } from "@mui/material";
import { isDatePastNow, isEndDateLater, isValidDescription, isValidMinInc, isValidTitle } from "./Validations";
import { DateTimePicker, LocalizationProvider } from "@mui/x-date-pickers";
import { AdapterDayjs } from '@mui/x-date-pickers/AdapterDayjs';
import { useTheme } from '@mui/material/styles';
import 'dayjs/locale/lt';
import { checkTokenValidity, refreshAccessToken } from "../../services/AuthenticationService";
import PATHS from "../../utils/Paths";
import { postAuction } from "../../services/AuctionService";
import { getCategories } from "../../services/PartCategoryService";
import { getParts } from "../../services/PartService";
import { postPartRequest } from "../../services/PartRequestService";

function CreateAuction() {
    const navigate = useNavigate();
    const theme = useTheme();
    const { role, setLogin, setLogout } = useUser();
    const openSnackbar = useContext(SnackbarContext);

    const [categories, setCategories] = useState([]);
    const [category, setCategory] = useState("");
    const [categoryParts, setCategoryParts] = useState([]);
    const [part, setPart] = useState(null);

    const [condition, setCondition] = useState("New");
    const [selectedImage, setSelectedImage] = useState(null);
    const [imageType, setImageType] = useState(null);
    const [imageFile, setImageFile] = useState(undefined);
    const [checkBox, setCheckbox] = useState(false);
    const [validationErrors, setValidationErrors] = useState({
        title: null,
        description: null,
        startDate: null,
        endDate: null,
        minInc: null,
        condition: null,
        image: null,
        part: null,
        partName: null
    });

    const handleCategoryChange = async (e) => {
        const categoryName = e.target.value;
        setCategory(categoryName);

        await fetchCategoryParts(categoryName);
    }

    const handlePartChange = async (part) => {
        setPart(part);
    }

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
      };

    const handleSubmit = async (e) => {
        e.preventDefault();

        const title = e.target.title.value;
        const description = e.target.description.value;
        const startDate = e.target.startDate.value;
        const endDate = e.target.endDate.value;
        const minInc = e.target.minInc.value;
        const manufacturer = e.target.manufacturer.value ? e.target.manufacturer.value : "";
        const partName = e.target.partname?.value ? e.target.partname.value : "";

        let errors = [];
        setValidationErrors(errors);
        if (!isValidTitle(title)) errors.title = "Title must be 5 - 45 characters long";
        if (!isValidDescription(description)) errors.description = "Description must be at least 10 characters long";
        if(!isDatePastNow(startDate)) errors.startDate = "Start date must be later than current time";
        if(!isDatePastNow(endDate)) errors.endDate = "End date must be later than current time";
        if(!isEndDateLater(startDate, endDate)) errors.endDate = "End date must be later than start date";
        if(!isValidMinInc(minInc)) errors.minInc = "Minimum increment must 0 or a positive number";
        if(startDate == null || startDate === "") errors.startDate = "Auction start date must be set";
        if(endDate == null || endDate === "") errors.endDate = "Auction end date must be set";
        if(condition === null || condition === "") errors.condition = "Condition must be set"
        if(selectedImage === null || selectedImage === "") errors.image = "Image must be selected";
        if(!imageType || !imageType.startsWith('image/')) errors.image = "Select a valid image file";
        if((part === "" || part === undefined || part == null) && checkBox === false) errors.part = "Select a part for this auction";
        if(partName === "" && checkBox === true) errors.partName = "Part name must be set";

        if (Object.keys(errors).length > 0) {
            setValidationErrors(errors);
            return;
        }

        const startDateLocal = new Date(startDate);
        const isoStartDate = startDateLocal.toISOString()
        const endDateLocal = new Date(endDate);
        const isoEndDate = endDateLocal.toISOString();

        const formData = new FormData();
        formData.append("name", title);
        formData.append("description", description);
        formData.append("startDate", isoStartDate);
        formData.append("endDate", isoEndDate);
        formData.append("minIncrement", minInc);
        formData.append("condition", condition);
        formData.append("manufacturer", manufacturer);
        formData.append("image", imageFile);
        formData.append("partCategory", category)
        formData.append("partId", part ? part.id : "");
        formData.append("partName", partName ? partName: "");
        formData.append("partCategoryName", category);

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

        try {
            const result = await postAuction(formData);

            if(partName)
            {
                const postData = {name: partName, auctionId: result.data.id, categoryId: category, partId: result.data.partId }
                await postPartRequest(postData);
            }
            navigate(PATHS.AUCTIONINFO.replace(":auctionId", result.data.id));
            openSnackbar('Auction created successfully!', 'success');
        } catch(error) {
            openSnackbar(error.response.data.errorMessage, "error")
        }
    };

    const fetchCategoryParts = async (categoryId) => {
        const result = await getParts(categoryId);
        setCategoryParts(result);
    }

    const handleCheckboxChange = (event) => {
        setCheckbox(event.target.checked);
      };

    useEffect(() => {
        if (!role.includes("RegisteredUser")) {
            openSnackbar('You must login to create auctions!', 'error');
            navigate(PATHS.LOGIN);
        }

        const fetchCategoriesData = async () => {
            const result = await getCategories();
            setCategories(result);

            const defaultCategory = "CPU";
            setCategory(defaultCategory);

            await fetchCategoryParts("CPU");
        };


        fetchCategoriesData();
    }, [navigate, openSnackbar, role]);

    return (
        <Container component="main" maxWidth="md">
            <CssBaseline />
            <Box
                sx={{
                    marginTop: 4,
                    display: 'flex',
                    flexDirection: 'column',
                    alignItems: 'center',
                }}
            >
                <Typography component="h1" variant="h5" sx={{ fontSize: '26px', fontWeight: 'bold', fontFamily: 'Arial, sans-serif', color: '#0d6267' }}>
                    CREATE NEW AUCTION
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
                            <FormControl fullWidth>
                                <InputLabel id="part-category-label">Part category *</InputLabel>
                                <Select
                                    labelId="part-category-label"
                                    id="part-category"
                                    label="Part category"
                                    value={category}
                                    onChange={handleCategoryChange}
                                    required
                                    sx={{ textAlign: 'left' }}
                                >
                                    {categories.map((category) => (
                                        <MenuItem key={category.id} value={category.id}>
                                            {category.id}
                                        </MenuItem>
                                    ))}
                                </Select>
                            </FormControl>
                        </Grid>
                        { checkBox === false ? (
                        <>
                        <Grid item xs={12}>
                            <FormControl fullWidth>
                            <Autocomplete
                                id="part-autocomplete"
                                options={categoryParts.map(p => ({ id: p.id, label: p.name }))}
                                getOptionLabel={(option) => option?.label || option}
                                value={part}
                                onChange={(event, newValue) => {
                                    handlePartChange(newValue);
                                }}
                                required
                                renderInput={(params) => <TextField {...params} label="Part *" error={Boolean(validationErrors.part)} helperText={validationErrors.part}  />}
                                isOptionEqualToValue={(option, value) => { return true; }}
                            />
                            </FormControl>
                        </Grid>
                        </> ) : (
                        <>
                            <Grid item xs={12}>
                                <TextField
                                    error={Boolean(validationErrors.partName)}
                                    helperText={validationErrors.partName}
                                    fullWidth
                                    id="partname"
                                    label="Part name"
                                    name="partname"
                                />
                            </Grid>
                        </>) }
                        <Grid item xs={12} container alignItems="center">
                            <FormControlLabel
                                control={<Checkbox checked={checkBox} onChange={handleCheckboxChange} />}
                                label={<Typography variant="body1" style={{ fontSize: '1.2rem', marginTop: 0 }}>Part not in the list</Typography>}
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

                        <Grid item xs={12} style={{ height: '30vh', display: 'flex', justifyContent: 'center', alignItems: 'center' }}>
                            {selectedImage ? (
                            <div style={{ width: '100%', height: '90%', display: 'flex', justifyContent: 'center', alignItems: 'center' }}>
                                <img src={selectedImage} alt="Selected" style={{ maxWidth: '100%', maxHeight: '100%', objectFit: 'contain' }} />
                            </div>
                            ) : (
                                <Skeleton variant="rectangular" width={'100%'} height={'100%'} />
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
                                <Button variant="contained" component="span" fullWidth sx={{ mt: 1, mb: 2, bgcolor: '#138c94', '&:hover': { backgroundColor: '#07383b'}  }}>
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
                        Create auction
                    </Button>
                </Box>
            </Box>
        </Container>
    );
}

export default CreateAuction;
