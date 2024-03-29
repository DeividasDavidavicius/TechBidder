import { Avatar, Box, Button, Container, CssBaseline, Grid, TextField, Typography } from "@mui/material";
import VpnKeyTwoToneIcon from '@mui/icons-material/VpnKeyTwoTone';
import { useNavigate } from "react-router-dom";
import { useUser } from "../../contexts/UserContext";
import { useContext, useEffect, useState } from "react";
import SnackbarContext from "../../contexts/SnackbarContext";
import { login, register } from "../../services/AuthenticationService";
import PATHS from "../../utils/Paths";

function Register() {
    const navigation = useNavigate();
    const { setLogin } = useUser();
    const [errorMessage, setErrorMessage] = useState(null);
    const openSnackbar = useContext(SnackbarContext);

    useEffect(() => {
        const isLoggedIn = localStorage.getItem('isLoggedIn');
        if (isLoggedIn === 'true') {
            openSnackbar('You have already logged in!', 'error');
            navigation(PATHS.MAIN);
        }
    });

    const handleSubmit = (e) => {
        e.preventDefault();

        const username = e.target.username.value;
        const password = e.target.password.value;
        const email = e.target.email.value;

        const isValidUsername = (username) => {
            return username && username.length >= 5;
        }

        const isValidPassword = (password) => {
            const re = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{6,}$/;
            return re.test(password);
        }

        const isValidEmail = (email) => {
            const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
            return emailRegex.test(email);
        };

        let errors = {};
        setValidationErrors(errors);
        if (!isValidUsername(username)) errors.username = "Username must be at least 5 characters long.";
        if (!isValidPassword(password)) errors.password = "Password must be at least 6 characters long, have a lowercase and uppercase letter, one number and symbol";
        if (!isValidEmail(email)) errors.email = "This is not a valid email";

        if (Object.keys(errors).length > 0) {
            setValidationErrors(errors);
            return;
        }

        tryRegister(username, password, email);
    }

    const tryRegister = async (username, password, email) => {
        try {
            const registerData = {username, password, email};
            const response = await register(registerData);

            if(response.status === 201) {
                const loginData = {username, password};
                const response2 = await login(loginData);
                setLogin(response2.data.accessToken, response2.data.refreshToken);
                navigation(PATHS.MAIN);
                openSnackbar('Succesfully registered!', 'success');
            }
            else {
                const errorMessage = await response.text;
                setErrorMessage(errorMessage);
            }
        } catch (error) {
            console.error("Login failed:", error);
            setErrorMessage(error.response.data);
        }
    }

    const [validationErrors, setValidationErrors] = useState({
        username: null,
        password: null
    });

    return (
        <Container component="main" maxWidth="xs">
            <CssBaseline />
            <Box
                sx={{
                    marginTop: 8,
                    display: 'flex',
                    flexDirection: 'column',
                    alignItems: 'center',
                }}
            >
                <Avatar sx={{ m: 1, bgcolor: '#ec736b' }}>
                    <VpnKeyTwoToneIcon />
                </Avatar>
                <Typography component="h1" variant="h5">
                    Register
                </Typography>
                {errorMessage &&
                    <Typography color="error">
                        {errorMessage}
                    </Typography>
                }
                <Box component="form" noValidate onSubmit={(event) => handleSubmit(event)} sx={{ mt: 3 }}>
                    <Grid container spacing={2}>
                        <Grid item xs={12}>
                            <TextField
                                error={Boolean(validationErrors.username)}
                                helperText={validationErrors.username}
                                required
                                fullWidth
                                id="username"
                                label="Username"
                                name="username"
                                autoComplete="username"
                                autoFocus
                            />
                        </Grid>
                        <Grid item xs={12}>
                            <TextField
                                error={Boolean(validationErrors.email)}
                                helperText={validationErrors.email}
                                required
                                fullWidth
                                id="email"
                                label="Email Address"
                                name="email"
                                autoComplete="email"
                            />
                        </Grid>
                        <Grid item xs={12}>
                            <TextField
                                error={Boolean(validationErrors.password)}
                                helperText={validationErrors.password}
                                required
                                fullWidth
                                name="password"
                                label="Password"
                                type="password"
                                id="password"
                                autoComplete="new-password"
                            />
                        </Grid>
                    </Grid>
                    <Button
                        type="submit"
                        fullWidth
                        variant="contained"
                        sx={{ mt: 3, mb: 2, bgcolor: '#0d6267', '&:hover': { backgroundColor: '#3d8185'}}}
                    >
                        Register
                    </Button>
                </Box>
            </Box>
        </Container>
    );
}

export default Register;
