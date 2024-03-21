import { Box, Button, Container, CssBaseline, Dialog, DialogActions, DialogTitle, Table, TableBody, TableCell, TableHead, TableRow, TextField } from "@mui/material";
import { useContext, useEffect, useState } from "react";
import { getCategories } from "../../services/PartCategoryService";
import ModeEditIcon from '@mui/icons-material/ModeEdit';
import DeleteIcon from '@mui/icons-material/Delete';
import AddCircleOutlineIcon from '@mui/icons-material/AddCircleOutline';
import HighlightOffIcon from '@mui/icons-material/HighlightOff';
import PATHS from "../../utils/Paths";
import { Link, useNavigate } from "react-router-dom";
import { checkTokenValidity, refreshAccessToken } from "../../services/AuthService";
import SnackbarContext from "../../contexts/SnackbarContext";
import { useUser } from "../../contexts/UserContext";
import { deleteSeries, getAllCategorySeries } from "../../services/SeriesService";

function SeriesList() {
    const [series, setSeries] = useState([]);
    const [currentSeries, setCurrentSeries] = useState({});
    const [searchTerm, setSearchTerm] = useState('');
    const [openRemoveModal, setOpenRemoveModal] = useState(false);

    const openSnackbar = useContext(SnackbarContext);
    const navigate = useNavigate();
    const { setLogin, setLogout } = useUser();

    useEffect(() => {
        const fetchCategoriesData = async () => {
            const result = await getCategories();

            const categoryIds = result.map(category => category.id);
            fetchSeriesData(categoryIds);
        };

        const fetchSeriesData = async (categoryIds) => {
            if (categoryIds.length === 0) return;

            const seriesPromises = categoryIds.map(category => getAllCategorySeries(category));
            const results = await Promise.all(seriesPromises);

            const flattenedSeries = results.reduce((acc, curr) => acc.concat(curr), []);

            setSeries(flattenedSeries);
        };

        fetchCategoriesData();

    }, []);

    const handleSearchChange = (event) => {
        setSearchTerm(event.target.value);
    };

    const filteredSeries = series.filter(s =>
        s.name.toLowerCase().includes(searchTerm.toLowerCase())
    );

    const handleOpenRemove = (series) => {
        setCurrentSeries(series);
        setOpenRemoveModal(true);
    };

    const handleCloseRemove = () => {
        setOpenRemoveModal(false);
        setCurrentSeries({});
    };

    const handleRemoveSeries = async () => {
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

        deleteSeries(currentSeries.categoryId, currentSeries.id);
        openSnackbar('Series deleted successfully!', 'success');

        const updatedSeries = series.filter(
            (s) => s.id !== currentSeries.id
        );
        setSeries(updatedSeries);
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
            <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', marginBottom: 2 }}>

                <TextField
                    label="Search Series"
                    variant="outlined"
                    value={searchTerm}
                    onChange={handleSearchChange}
                    fullWidth
                    margin="none"
                    size="small"
                    sx={{ width: 'calc(50% - 40px)' }}
                />
                <Link to={PATHS.CREATESERIES}>
                    <Button startIcon={<AddCircleOutlineIcon />} sx={{ color: '#138c94', fontWeight: 'bold' }}>
                        CREATE SERIES
                    </Button>
                </Link>
            </Box>
            <Table size="small">
                <TableHead>
                    <TableRow>
                        <TableCell style={{ backgroundColor: '#0d6267', color: 'white', fontWeight: 'bold' }}>CATEGORY</TableCell>
                        <TableCell style={{ backgroundColor: '#0d6267', color: 'white', fontWeight: 'bold' }}>SERIES NAME</TableCell>
                        <TableCell style={{ backgroundColor: '#0d6267',  color: 'white', fontWeight: 'bold' }}>ACTIONS</TableCell>
                    </TableRow>
                </TableHead>
                <TableBody>
                {filteredSeries.map((s, index) => (
                    <TableRow key={index}>
                        <TableCell>{s.categoryId}</TableCell>
                        <TableCell>{s.name}</TableCell>
                        <TableCell>
                            <Link to={PATHS.EDITSERIES.replace(':seriesId', s.id).replace(':categoryId', s.categoryId)}>
                                <Button startIcon={<ModeEditIcon />} sx={{ marginRight: 3, color: '#138c94', fontWeight: 'bold' }}>
                                    Edit
                                </Button>
                            </Link>
                            <Button startIcon={<DeleteIcon />}
                                sx={{ marginRight: 0, color: '#138c94', fontWeight: 'bold' }}
                                onClick={ () => handleOpenRemove(s)}
                            >
                                Delete
                            </Button>
                        </TableCell>
                    </TableRow>
                ))}
                </TableBody>
            </Table>
        </Box>
        <Dialog open={openRemoveModal} onClose={handleCloseRemove}>
                <DialogTitle sx={{ fontSize: '20px', fontWeight: 'bold', fontFamily: 'Arial, sans-serif', color: '#0d6267' }} >Do you want to remove '{currentSeries.name}'?</DialogTitle>
                <DialogActions style={{ justifyContent: 'center' }}>
                    <Button onClick={handleRemoveSeries} startIcon={<ModeEditIcon />} sx ={{ fontWeight: 'bold', color: "red" }}>
                        Remove
                    </Button>
                    <Button onClick={handleCloseRemove} startIcon={<HighlightOffIcon />} sx ={{ fontWeight: 'bold', color: "#369c7d" }}>
                        Cancel
                    </Button>
                </DialogActions>
            </Dialog>
      </Container>
      );
}

export default SeriesList;
