import { Box, Button, Container, CssBaseline, Dialog, DialogActions, DialogTitle, Table, TableBody, TableCell, TableHead, TableRow } from "@mui/material";
import { useContext, useEffect, useState } from "react";
import { getCategories } from "../../services/PartCategoryService";
import { deletePart, getPartRequests } from "../../services/PartService";
import ModeEditIcon from '@mui/icons-material/ModeEdit';
import HighlightOffIcon from '@mui/icons-material/HighlightOff';
import AddIcon from '@mui/icons-material/Add';
import PATHS from "../../utils/Paths";
import { Link, useNavigate } from "react-router-dom";
import { checkTokenValidity, refreshAccessToken } from "../../services/AuthenticationService";
import SnackbarContext from "../../contexts/SnackbarContext";
import { useUser } from "../../contexts/UserContext";

function PartRequestList() {
    const [parts, setParts] = useState([]);
    const [currentPart, setCurrentPart] = useState({});
    const [openRemoveModal, setOpenRemoveModal] = useState(false);

    const openSnackbar = useContext(SnackbarContext);
    const navigate = useNavigate();
    const { role, setLogin, setLogout } = useUser();

    useEffect(() => {
        if (!role.includes("Admin")) {
            openSnackbar('Only admins can access this page!', 'error');
            navigate(PATHS.MAIN);
        }

        const fetchCategoriesData = async () => {
            const result = await getCategories();

            const categoryIds = result.map(category => category.id);
            fetchPartsData(categoryIds);
        };

        const fetchPartsData = async (categoryIds) => {
            if (categoryIds.length === 0) return;

            const partsPromises = categoryIds.map(category => getPartRequests(category));
            const results = await Promise.all(partsPromises);

            const flattenedParts = results.reduce((acc, curr) => acc.concat(curr), []);

            setParts(flattenedParts);
        };

        fetchCategoriesData();

    }, [navigate, openSnackbar, role]);

    const handleCloseRemove = () => {
        setOpenRemoveModal(false);
        setCurrentPart({});
    };

    const handleRemovePart = async () => {
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

        deletePart(currentPart.categoryId, currentPart.id);
        openSnackbar('Part deleted successfully!', 'success');

        const updatedParts = parts.filter(
            (part) => part.id !== currentPart.id
        );
        setParts(updatedParts);
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
            <Table size="small">
                <TableHead>
                    <TableRow>
                        <TableCell style={{ backgroundColor: '#0d6267', color: 'white', fontWeight: 'bold' }}>CATEGORY</TableCell>
                        <TableCell style={{ backgroundColor: '#0d6267', color: 'white', fontWeight: 'bold' }}>PART NAME</TableCell>
                        <TableCell style={{ backgroundColor: '#0d6267',  color: 'white', fontWeight: 'bold' }}>ACTIONS</TableCell>
                    </TableRow>
                </TableHead>
                <TableBody>
                {parts.map((part, index) => (
                    <TableRow key={index}>
                        <TableCell>{part.categoryId}</TableCell>
                        <TableCell>{part.name}</TableCell>
                        <TableCell>
                            <Link to={PATHS.PARTREQUESTCREATE.replace(':partId', part.id).replace(':categoryId', part.categoryId)}>
                                <Button startIcon={<AddIcon />} sx={{ marginRight: 3, color: '#138c94', fontWeight: 'bold' }}>
                                    CREATE
                                </Button>
                            </Link>
                        </TableCell>
                    </TableRow>
                ))}
                </TableBody>
            </Table>
        </Box>
        <Dialog open={openRemoveModal} onClose={handleCloseRemove}>
                <DialogTitle sx={{ fontSize: '20px', fontWeight: 'bold', fontFamily: 'Arial, sans-serif', color: '#0d6267' }} >Do you want to remove '{currentPart.name}'?</DialogTitle>
                <DialogActions style={{ justifyContent: 'center' }}>
                    <Button onClick={handleRemovePart} startIcon={<ModeEditIcon />} sx ={{ fontWeight: 'bold', color: "red" }}>
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
