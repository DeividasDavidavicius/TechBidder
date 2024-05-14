import { Box, Button, Container, CssBaseline, Dialog, DialogActions, DialogTitle, Table, TableBody, TableCell, TableHead, TableRow, Typography } from "@mui/material";
import { useContext, useEffect, useState } from "react";
import ModeEditIcon from '@mui/icons-material/ModeEdit';
import HighlightOffIcon from '@mui/icons-material/HighlightOff';
import DeleteIcon from '@mui/icons-material/Delete';
import InfoOutlinedIcon from '@mui/icons-material/InfoOutlined';
import AddIcon from '@mui/icons-material/Add';
import PATHS from "../../utils/Paths";
import { Link, useNavigate } from "react-router-dom";
import { checkTokenValidity, refreshAccessToken } from "../../services/AuthenticationService";
import SnackbarContext from "../../contexts/SnackbarContext";
import { useUser } from "../../contexts/UserContext";
import { deletePartRequest, getAllPartRequests } from "../../services/PartRequestService";

function PartRequestList() {
    const [requests, setRequests] = useState([]);
    const [currentRequest, setCurrentRequest] = useState({});
    const [openRemoveModal, setOpenRemoveModal] = useState(false);

    const openSnackbar = useContext(SnackbarContext);
    const navigate = useNavigate();
    const { role, setLogin, setLogout } = useUser();

    useEffect(() => {
        if (!role.includes("Admin")) {
            openSnackbar('Only admins can access this page!', 'error');
            navigate(PATHS.MAIN);
        }

        const fetchPartRequests = async () => {
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

            const result = await getAllPartRequests();
            setRequests(result);
        };


        fetchPartRequests();

    }, [navigate, openSnackbar, role, setLogin, setLogout]);

    const handleOpenRemove = (request) => {
        setCurrentRequest(request);
        setOpenRemoveModal(true);
    };

    const handleCloseRemove = () => {
        setOpenRemoveModal(false);
        setCurrentRequest({});
    };

    const handleRemovePartRequest = async () => {
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

        deletePartRequest(currentRequest.id);
        openSnackbar('Part request deleted successfully!', 'success');

        const updatedRequests = requests.filter(
            (request) => request.id !== currentRequest.id
        );
        setRequests(updatedRequests);
        handleCloseRemove();
    }

    return (

        <Container component="main" maxWidth="md">
        <CssBaseline />
        <Box
          sx={{
            marginTop: 8,
            border: '1px solid #ccc',
            borderRadius: '10px',
            padding: '10px',
          }}
        >
            <Typography component="h1" variant="h5" sx={{ marginBottom: 1, fontSize: '26px', fontWeight: 'bold', fontFamily: 'Arial, sans-serif', color: '#0d6267' }}>
                    PART REQUESTS
            </Typography>
            <Table size="small">
                <TableHead>
                    <TableRow>
                        <TableCell style={{ backgroundColor: '#0d6267', color: 'white', fontWeight: 'bold' }}>CATEGORY</TableCell>
                        <TableCell style={{ backgroundColor: '#0d6267', color: 'white', fontWeight: 'bold' }}>PART NAME</TableCell>
                        <TableCell style={{ backgroundColor: '#0d6267',  color: 'white', fontWeight: 'bold' }}>ACTIONS</TableCell>
                    </TableRow>
                </TableHead>
                <TableBody>
                {requests.map((request, index) => (
                    <TableRow key={index} >
                        <TableCell style={{ width: '20%' }}>{request.categoryId}</TableCell>
                        <TableCell style={{ width: '33%' }}>{request.name}</TableCell>
                        <TableCell style={{ width: '48%' }}>
                            <Link to={PATHS.PARTREQUESTCREATE.replace(':requestId', request.id).replace(':categoryId', request.categoryId)}>
                                <Button startIcon={<AddIcon />} sx={{ marginRight: 2, color: '#138c94', fontWeight: 'bold' }}>
                                    ACCEPT
                                </Button>
                            </Link>
                            <Button startIcon={<DeleteIcon />}
                                sx={{ marginRight: 2, color: '#138c94', fontWeight: 'bold' }}
                                onClick={ () => handleOpenRemove(request)}
                            >
                                Delete
                            </Button>
                            <Link to={PATHS.AUCTIONINFO.replace(':auctionId', request.auctionId).replace(':categoryId', request.categoryId)}>
                                <Button startIcon={<InfoOutlinedIcon />} sx={{ marginRight: 2, color: '#138c94', fontWeight: 'bold' }}>
                                    AUCTION
                                </Button>
                            </Link>
                        </TableCell>
                    </TableRow>
                ))}
                </TableBody>
            </Table>
        </Box>
        <Dialog open={openRemoveModal} onClose={handleCloseRemove}>
                <DialogTitle sx={{ fontSize: '20px', fontWeight: 'bold', fontFamily: 'Arial, sans-serif', color: '#0d6267' }} >Do you want to remove '{currentRequest.name}'?</DialogTitle>
                <DialogActions style={{ justifyContent: 'center' }}>
                    <Button onClick={handleRemovePartRequest} startIcon={<ModeEditIcon />} sx ={{ fontWeight: 'bold', color: "red" }}>
                        Remove
                    </Button>
                    <Button onClick={handleCloseRemove} startIcon={<HighlightOffIcon />} sx ={{ fontWeight: 'bold', color: "#268747" }}>
                        Cancel
                    </Button>
                </DialogActions>
            </Dialog>
      </Container>
      );
}

export default PartRequestList;
