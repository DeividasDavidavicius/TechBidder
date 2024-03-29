import { AppBar, Avatar, Box, Button, Container, IconButton, Menu, MenuItem, Toolbar, Tooltip } from "@mui/material";
import MenuIcon from '@mui/icons-material/Menu';
import RegisterIcon from '@mui/icons-material/PersonAdd';
import LoginIcon from '@mui/icons-material/Login';
import SettingsIcon from '@mui/icons-material/Settings';
import SnackbarContext from "../contexts/SnackbarContext";
import { useContext, useEffect, useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import './../App.css';
import { useUser } from "../contexts/UserContext";
import { checkTokenValidity, logout, refreshAccessToken } from "../services/AuthenticationService";
import PATHS from "../utils/Paths";

function Header() {
    const { isLoggedIn, accessToken, setLogin, setLogout } = useUser();
    const openSnackbar = useContext(SnackbarContext);
    const navigation = useNavigate();

    const pages = [
        { name: 'MAIN', route: PATHS.MAIN },
        { name: 'CREATE', route: PATHS.CREATEAUCTION },
        { name: "AUCTIONS", route: PATHS.AUCTIONS + "?page=1" },
        { name: "PARTS", route: PATHS.PARTS },
        { name: "SERIES", route: PATHS.SERIES }
    ];

    let navOptions = [];

    if (isLoggedIn === true) {
        navOptions.push({ name: 'Logout', route: PATHS.MAIN, method: 'handleLogout' });
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
                navigation(PATHS.MAIN);
                return;
            }
            setLogin(result.response.data.accessToken, result.response.data.refreshToken);
        }

        try {
            const response = await logout(localStorage.getItem('accessToken'), localStorage.getItem('refreshToken'));
            if (response.status === 200) {
                setLogout(accessToken);
                navigation(PATHS.LOGIN);
                openSnackbar('Succesfully logged out!', 'success');
            }
        } catch { }
    };

    useEffect(() => {

        const refreshToken = localStorage.getItem('refreshToken');
        if(refreshToken === null || refreshToken === "empty")
            return;
        else if(!checkTokenValidity(refreshToken))
        {
            setLogout();
            navigation(PATHS.LOGIN);
        }
    }, [navigation, setLogout]);

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
                        {pages.map((page, index) => (
                            <Link key={page.name} to={page.route} style={{ textDecoration: 'none', color: 'white', fontWeight: 'bold', colorborderBottom: '2px solid transparent', marginRight: index !== pages.length - 1 ? '15px' : 0}} className="header-link" >
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
                                    <Link to={PATHS.LOGIN}>
                                        <Button startIcon={<LoginIcon />} sx={{ marginRight: 1, color: 'white', fontWeight: 'bold' }}>
                                            Login
                                        </Button>
                                    </Link>
                                    <Link to={PATHS.REGISTER}>
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
