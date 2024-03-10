import { AppBar, Avatar, Box, Button, Container, IconButton, Menu, MenuItem, Toolbar, Tooltip } from "@mui/material";
import MenuIcon from '@mui/icons-material/Menu';
import RegisterIcon from '@mui/icons-material/PersonAdd';
import LoginIcon from '@mui/icons-material/Login';
import SettingsIcon from '@mui/icons-material/Settings';
import SnackbarContext from "../contexts/SnackbarContext";
import { useContext, useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import './../App.css';
import { useUser } from "../contexts/UserContext";
import { checkTokenValidity, logout, refreshAccessToken } from "../services/authService";

function Header() {
    const { isLoggedIn, role, accessToken, setLogin, setLogout } = useUser();
    const openSnackbar = useContext(SnackbarContext);
    const navigation = useNavigate();

    const pages = [
        { name: 'MAIN', route: '/' }
    ];

    let navOptions = [];

    if (isLoggedIn === true) {
        navOptions.push({ name: 'Logout', route: '/', method: 'handleLogout' });
    }

    const [anchorElNav, setAnchorElNav] = useState(null);
    const [anchorElUser, setAnchorElUser] = useState(null);

    const handleNavigation = (url) => {
        navigation(url);
    };

    const handleOpenNavMenu = (event) => {
        setAnchorElNav(event.currentTarget);
    };

    const handleCloseNavMenu = () => {
        setAnchorElNav(null);
    };

    const handleOpenUserMenu = (event) => {
        setAnchorElUser(event.currentTarget);
    };

    const handleCloseUserMenu = () => {
        setAnchorElUser(null);
    };

    function handleMenuItemClick(setting) {
        handleCloseUserMenu();
        if (setting.method) {
            if (setting.method === 'handleLogout') {
                handleLogout();
            }
        }
    }

    async function handleLogout() {
        if (!checkTokenValidity(accessToken)) {
            const result = await refreshAccessToken();
            if (!result.success) {
                setLogout();
                navigation('/');
                return;
            }
            setLogin(result.response.data.accessToken, result.response.data.refreshToken);
        }

        try {
            const response = await logout(localStorage.getItem('accessToken'), localStorage.getItem('refreshToken'));
            if (response.status === 200) {
                setLogout(accessToken);
                navigation('/login');
                openSnackbar('Succesfully logged out!', 'success');
            }
        } catch { }
    }

    return (
        <AppBar position="sticky" style={{ top: 0, zIndex: 1000, backgroundColor: '#138c94'}}>
            <Container maxWidth="x1">
                <Toolbar disableGutters>
                    <Box sx={{ flexGrow: 1, display: { xs: 'flex', md: 'none' } }}>
                        <IconButton color="inherit" onClick={handleOpenNavMenu}>
                            <MenuIcon/>
                        </IconButton>
                        <Menu
                            id="menu-appbar"
                            anchorEl={anchorElNav}
                            anchorOrigin={{
                                vertical: 'bottom',
                                horizontal: 'left',
                            }}
                            keepMounted
                            transformOrigin={{
                                vertical: 'top',
                                horizontal: 'left',
                            }}
                            open={Boolean(anchorElNav)}
                            onClose={handleCloseNavMenu}
                            sx={{
                                display: { xs: 'block', md: 'none' },
                            }}
                        >
                            {pages.map((page) => (
                                <MenuItem
                                    key={page.name}
                                    onClick={() => handleNavigation(page.route)}
                                    style={{ cursor: 'pointer', backgroundColor: 'transparent', border: 'none' }}
                                >
                                    <Link to={page.route} style={{ textDecoration: 'none', color: 'black' }}>
                                        {page.name}
                                    </Link>
                                </MenuItem>
                            ))}
                        </Menu>
                    </Box>

                    <Box sx={{ flexGrow: 1, display: { xs: 'none', md: 'flex' } }}>
                        {pages.map((page) => (
                            <Link key={page.name} to={page.route} style={{ textDecoration: 'none', color: 'white', fontWeight: 'bold', colorborderBottom: '2px solid transparent'}} className="header-link" >
                                {page.name}
                            </Link>
                        ))}
                    </Box>

                    <Box sx={{ flexGrow: 0 }}>
                        {isLoggedIn ?
                            <>
                                <Tooltip title="Open settings">
                                    <IconButton onClick={handleOpenUserMenu} sx={{ p: 0 }}>
                                        <Avatar sx={{ m: 1, bgcolor: '#1a4c4f' }}>
                                            <SettingsIcon />
                                        </Avatar>
                                    </IconButton>
                                </Tooltip>
                                <Menu
                                    sx={{ mt: '45px' }}
                                    id="menu-appbar"
                                    anchorEl={anchorElUser}
                                    anchorOrigin={{
                                        vertical: 'top',
                                        horizontal: 'right',
                                    }}
                                    keepMounted
                                    transformOrigin={{
                                        vertical: 'top',
                                        horizontal: 'right',
                                    }}
                                    open={Boolean(anchorElUser)}
                                    onClose={handleCloseUserMenu}
                                >
                                    {navOptions.map((setting) => (
                                        <MenuItem
                                            key={setting.name}
                                            onClick={() => {
                                                handleMenuItemClick(setting);
                                                handleNavigation(setting.route);
                                            }}
                                            style={{
                                                cursor: 'pointer',
                                                pointerEvents: 'auto',
                                            }}
                                        >
                                            <span style={{ textDecoration: 'none', color: 'black' }}>
                                                {setting.name}
                                            </span>
                                        </MenuItem>
                                    ))}
                                </Menu>
                            </> :
                            <>
                                <Box sx={{ display: 'flex', flexDirection: 'row', alignItems: 'center' }}>
                                    <Link to="/login">
                                        <Button startIcon={<LoginIcon />} sx={{ marginRight: 1, color: 'white', fontWeight: 'bold' }}>
                                            Login
                                        </Button>
                                    </Link>
                                    <Link to="/register">
                                        <Button startIcon={<RegisterIcon />} sx={{ marginRight: 1, color: 'white', fontWeight: 'bold' }}>
                                            Register
                                        </Button>
                                    </Link>
                                </Box>
                            </>}
                    </Box>
                </Toolbar>
            </Container>
        </AppBar>
    );
}

export default Header;
