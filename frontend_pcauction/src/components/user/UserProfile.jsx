import { AppBar, Box, Container, CssBaseline, Tab, Tabs, Typography } from "@mui/material";
import { useUser } from "../../contexts/UserContext";
import { useState } from "react";

function UserProfile() {
    const [tabValue, setTabValue] = useState(0);
    const handleChange = (event, newValue) => {
        setTabValue(newValue);
      };

    const { role, setLogin, setLogout, getUserId, getUserName } = useUser();




    return (
        <Container component="main" maxWidth="lg">
            <CssBaseline />
            <Box
                sx={{
                marginTop: 8,
                border: '1px solid #ccc',
                borderRadius: '10px',
                }}
            >
                <Box sx={{ marginTop: 2, display: 'flex', flexDirection: 'column', alignItems: 'center' }}>
                    <Typography component="h1" variant="h5" sx={{ fontSize: '26px', fontWeight: 'bold', fontFamily: 'Arial, sans-serif', color: '#0d6267' }}>
                        User '{getUserName()}' profile
                    </Typography>
                </Box>

                <Box sx={{
                    display: 'flex',
                    flexDirection: 'column',
                    justifyContent: 'center',
                    alignItems: 'center',
                    marginTop: 2
                }}>
                    <Box sx={{ borderBottom: 1, borderColor: 'divider', width: '100%', maxWidth: '90%' }}>
                        <AppBar position="static" sx={{ backgroundColor: '#fff', color: '#000' }}>
                            <Tabs value={tabValue} onChange={handleChange} aria-label="basic tabs example">
                                <Tab label="Tab 1" />
                                <Tab label="Tab 2" />
                                <Tab label="Tab 3" />
                            </Tabs>
                        </AppBar>
                        {tabValue === 0 && (
                            <Box sx={{ p: 3 }}>
                            <h2>Tab 1 Content</h2>
                            <p>This is the content of Tab 1.</p>
                            </Box>
                        )}
                        {tabValue === 1 && (
                            <Box sx={{ p: 3 }}>
                            <h2>Tab 2 Content</h2>
                            <p>This is the content of Tab 2.</p>
                            </Box>
                        )}
                        {tabValue === 2 && (
                            <Box sx={{ p: 3 }}>
                            <h2>Tab 3 Content</h2>
                            <p>This is the content of Tab 3.</p>
                            </Box>
                        )}
                    </Box>
                </Box>
            </Box>
        </Container>
    );
}

export default UserProfile;
