import { Box, Button, Container, CssBaseline, Table, TableBody, TableCell, TableHead, TableRow, TextField } from "@mui/material";
import { useEffect, useState } from "react";
import { getCategories } from "../../services/PartCategoryService";
import { getParts } from "../../services/PartService";
import ModeEditIcon from '@mui/icons-material/ModeEdit';
import DeleteIcon from '@mui/icons-material/Delete';
import AddCircleOutlineIcon from '@mui/icons-material/AddCircleOutline';
import PATHS from "../../utils/Paths";
import { Link } from "react-router-dom";

function PartList() {
    const [parts, setParts] = useState([]);
    const [searchTerm, setSearchTerm] = useState('');

    useEffect(() => {
        const fetchCategoriesData = async () => {
            const result = await getCategories();

            const categoryIds = result.map(category => category.id);
            fetchPartsData(categoryIds);
        };

        const fetchPartsData = async (categoryIds) => {
            if (categoryIds.length === 0) return;

            const partsPromises = categoryIds.map(category => getParts(category));
            const results = await Promise.all(partsPromises);

            const flattenedParts = results.reduce((acc, curr) => acc.concat(curr), []);

            setParts(flattenedParts);
        };

        fetchCategoriesData();

    }, []);

    const handleSearchChange = (event) => {
        setSearchTerm(event.target.value);
    };

    const filteredParts = parts.filter(part =>
        part.name.toLowerCase().includes(searchTerm.toLowerCase())
    );


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
                    label="Search Parts"
                    variant="outlined"
                    value={searchTerm}
                    onChange={handleSearchChange}
                    fullWidth
                    margin="none"
                    size="small"
                    sx={{ width: 'calc(50% - 40px)' }}
                />
                <Link to={PATHS.CREATEPART}>
                    <Button startIcon={<AddCircleOutlineIcon />} sx={{ color: '#138c94', fontWeight: 'bold' }}>
                        CREATE PART
                    </Button>
                </Link>
            </Box>
            <Table size="small">
                <TableHead>
                    <TableRow>
                        <TableCell style={{ backgroundColor: '#0d6267', color: 'white', fontWeight: 'bold' }}>CATEGORY</TableCell>
                        <TableCell style={{ backgroundColor: '#0d6267', color: 'white', fontWeight: 'bold' }}>NAME</TableCell>
                        <TableCell style={{ backgroundColor: '#0d6267',  color: 'white', fontWeight: 'bold' }}>ACTIONS</TableCell>
                    </TableRow>
                </TableHead>
                <TableBody>
                {filteredParts.map((part, index) => (
                    <TableRow key={index}>
                        <TableCell>{part.categoryId}</TableCell>
                        <TableCell>{part.name}</TableCell>
                        <TableCell>
                            <Button startIcon={<ModeEditIcon />} sx={{ marginRight: 3, color: '#138c94', fontWeight: 'bold' }}>
                                Edit
                            </Button>
                            <Button startIcon={<DeleteIcon />} sx={{ marginRight: 0, color: '#138c94', fontWeight: 'bold' }}>
                                Delete
                            </Button>
                        </TableCell>
                    </TableRow>
                ))}
                </TableBody>
            </Table>
        </Box>
      </Container>
        );
}

export default PartList;
