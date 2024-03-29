import { useContext, useEffect, useState } from "react";
import { getCategories } from "../../services/PartCategoryService";
import { Box, Button, Container, CssBaseline, FormControl, Grid, InputLabel, MenuItem, Select, TextField, Typography } from "@mui/material";
import { checkTokenValidity, refreshAccessToken } from "../../services/AuthenticationService";
import { useNavigate } from "react-router-dom";
import { useUser } from "../../contexts/UserContext";
import SnackbarContext from "../../contexts/SnackbarContext";
import PATHS from "../../utils/Paths";
import { postSeries } from "../../services/SeriesService";

function CreateSeries() {
    const [categories, setCategories] = useState([]);
    const [category, setCategory] = useState("");
    const [name, setName] = useState("");

    const [validationErrors, setValidationErrors] = useState({
        name: null
    });

    const navigate = useNavigate();
    const { role, setLogin, setLogout } = useUser();
    const openSnackbar = useContext(SnackbarContext);

    const handleCategoryChange = (e) => {
        const categoryName = e.target.value;
        setCategory(categoryName);
    }

    const handleNameChange = (e) => {
        setName(e.target.value);
    };

    const handleSubmit = async (e) => {
        e.preventDefault();

        let errors = [];
        setValidationErrors(errors);
        if(!(name && name.length >= 0)) errors.name = "Series name must be set"

        if (Object.keys(errors).length > 0) {
            setValidationErrors(errors);
            return;
        }

        const postData = {name};

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
            await postSeries(postData, category);
            navigate(PATHS.SERIES);
            openSnackbar('Series created successfully!', 'success');
        } catch(error) {
            openSnackbar(error.response.data.errorMessage, "error")
        }

    };

    useEffect(() => {
        if (!role.includes("Admin")) {
            openSnackbar('Only admins can access this page!', 'error');
            navigate(PATHS.MAIN);
        }

        const fetchCategoriesData = async () => {
            const result = await getCategories();
            setCategories(result);

            const defaultCategory = "CPU";
            setCategory(defaultCategory);
        };

        fetchCategoriesData();

    }, [navigate, openSnackbar, role]);

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
                <Typography component="h1" variant="h5" sx={{ fontSize: '26px', fontWeight: 'bold', fontFamily: 'Arial, sans-serif', color: '#0d6267' }}>
                    CREATE NEW SERIES
                </Typography>

                <Box component="form" noValidate onSubmit={(event) => handleSubmit(event)} sx={{ mt: 3 }}>
                    <Grid container spacing={2}>
                        <Grid item xs={12}>
                            <FormControl fullWidth>
                                <InputLabel id="series-category-label">Series category *</InputLabel>
                                <Select
                                    labelId="series-category-label"
                                    id="series-category"
                                    label="Series category"
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
                        <Grid item xs={12}>
                            <TextField
                                error={Boolean(validationErrors.name)}
                                helperText={validationErrors.name}
                                required
                                fullWidth
                                id="name"
                                label="Name"
                                name="name"
                                autoFocus
                                onChange={handleNameChange}
                            />
                        </Grid>
                    </Grid>
                    <Button
                        type="submit"
                        fullWidth
                        variant="contained"
                        sx={{ mt: 2, mb: 2, bgcolor: '#0d6267', '&:hover': { backgroundColor: '#07383b'}}}
                    >
                        CREATE SERIES
                    </Button>
                </Box>

            </Box>
        </Container>
    );
};

export default CreateSeries;
