import { useContext, useEffect, useState } from "react";
import { Box, Button, Container, CssBaseline, Grid, TextField, Typography } from "@mui/material";
import { checkTokenValidity, refreshAccessToken } from "../../services/AuthService";
import { useNavigate, useParams } from "react-router-dom";
import { useUser } from "../../contexts/UserContext";
import SnackbarContext from "../../contexts/SnackbarContext";
import PATHS from "../../utils/Paths";
import { getSeries, putSeries } from "../../services/SeriesService";
function EditSeries() {
    const [name, setName] = useState("");
    const [validationErrors, setValidationErrors] = useState({
        name: null
    });

    const navigate = useNavigate();
    const { role, setLogin, setLogout } = useUser();
    const openSnackbar = useContext(SnackbarContext);
    const { seriesId } = useParams();
    const { categoryId } = useParams();

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

        const putData = {name};

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
            await putSeries(putData, categoryId, seriesId);
            navigate(PATHS.SERIES);
            openSnackbar('Series updated successfully!', 'success');
        } catch(error) {
            openSnackbar(error.response.data.errorMessage, "error")
        }

    };

    useEffect(() => {
        if (!role.includes("Admin")) {
            openSnackbar('Only admins can access this page!', 'error');
            navigate(PATHS.MAIN);
        }

        const fetchSeriesData = async () => {
            const seriesData = await getSeries(categoryId, seriesId);
            setName(seriesData.name);
        };

        fetchSeriesData();

    }, [categoryId, seriesId, navigate, openSnackbar, role]);

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
                    EDIT SERIES
                </Typography>

                <Box component="form" noValidate onSubmit={(event) => handleSubmit(event)} sx={{ mt: 3 }}>
                    <Grid container spacing={2}>
                        <Grid item xs={12}>
                            <TextField
                                fullWidth
                                id="series-category"
                                label="Series category"
                                value={categoryId}
                                name="series-category"
                                disabled={true}
                                sx={{
                                    "& .MuiInputBase-input.Mui-disabled": {
                                        WebkitTextFillColor: "#138c94",
                                        fontWeight: 'bold'
                                    },
                                    "& .MuiInputBase-root.Mui-disabled": {
                                        "& > fieldset": {
                                         borderColor: "#138c94"
                                        }
                                    },
                                    '& label.Mui-disabled': {
                                        color: '#138c94'
                                    },
                                }}
                            />
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
                                value={name}
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
                        UPDATE SERIES
                    </Button>
                </Box>

            </Box>
        </Container>
    );
};

export default EditSeries;
