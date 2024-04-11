import { Autocomplete, Box, Button, Card, CardContent, Container, CssBaseline, FormControl, Grid, TextField, Typography } from "@mui/material";
import { useEffect, useState } from "react";
import { getParts } from "../../services/PartService";
import { findCompatibleParts } from "../../services/CalculationsService";
import { Link } from "react-router-dom";
import PATHS from "../../utils/Paths";

function CompatibilityCheck() {

    const [categories] = useState([{id:'Motherboard'},{id: 'CPU'},{id: 'GPU'},{id:'HDD'}, {id: 'RAM'},{id: 'SSD'}]);
    const [compatibleCategories, setCompatibleCategories] = useState([]);
    const [parts, setParts] = useState([]);
    const [selectedCategory, setSelectedCategory] = useState(null);
    const [selectedCompCategory, setSelectedCompCategory] = useState(null);
    const [selectedPart, setSelectedPart] = useState(null);

    const [compatibleParts, setCompatibleParts] = useState(null);

    const [validationErrors, setValidationErrors] = useState({
        category: null,
        compCategory: null,
        part: null
    });

    const handleCategoryChange = async (category) => {
        await setSelectedCategory(category);
        await setSelectedPart(null);
        await setSelectedCompCategory(null);

        if(category === null) {
            setCompatibleCategories([]);
        }
        else if(category.id === 'Motherboard')
        {
            setCompatibleCategories([{id: 'CPU'},{id: 'GPU'},{id:'HDD'}, {id: 'RAM'},{id: 'SSD'}]);
        }
        else
        {
            setCompatibleCategories([{id: 'Motherboard'}]);
        }
    }

    const handlePartChange = async (part) => {
        await setSelectedPart(part);
    }

    const handleCompCategoryChange = async (category) => {
        await setSelectedCompCategory(category);
    }

    const getCompatibleParts = async (e) => {
        e.preventDefault();

        let errors = [];
        setValidationErrors(errors);
        if(selectedPart === null) errors.part = "Select part"
        if(selectedCategory === null) errors.category = "Select category"
        if(selectedCompCategory === null) errors.compCategory = "Select compatible part category"


        if (Object.keys(errors).length > 0) {
            setValidationErrors(errors);
            return;
        }

        const data = {
            categoryId: selectedCategory.id,
            partId: selectedPart.id,
            compatibleCategoryId: selectedCompCategory.id
        };

        const result = await findCompatibleParts(data);
        setCompatibleParts(result);
    };


    useEffect(() => {
        const fetchPartData = async () => {
            const partsPromises = categories.map(category =>
                getParts(category.id)
            );

            const partsData = await Promise.all(partsPromises);
            const flattenedPartData = partsData.flat();
            await setParts(flattenedPartData);
        }

        fetchPartData();

    }, [categories]);


    return (
        <Container component="main" maxWidth="md">
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
                    PART COMPATIBILITY
                </Typography>

                <Grid container spacing={2} sx={{marginTop: 2}}>
                    <Grid item xs={15} sm={4}>
                        <FormControl fullWidth>
                            <Autocomplete
                                id="categories-autocomplete"
                                options={categories.map(p => ({ id: p.id, label: p.id }))}
                                getOptionLabel={(option) => option?.label || option}
                                value={selectedCategory}
                                onChange={(event, newValue) => handleCategoryChange(newValue)}
                                required
                                renderInput={(params) => <TextField {...params} label="Part category *" error={Boolean(validationErrors.category)} helperText={validationErrors.category}/>}
                                isOptionEqualToValue={(option, value) => true}
                            />
                        </FormControl>
                    </Grid>
                    <Grid item xs={12} sm={4}>
                        <FormControl fullWidth>
                            <Autocomplete
                                id="parts-autocomplete"
                                options={
                                    selectedCategory
                                    ? parts.filter(p => p.categoryId === selectedCategory.id).map(p => ({ id: p.id, label: p.name }))
                                    : parts.map(p => ({ id: p.id, label: p.name }))
                                  }
                                getOptionLabel={(option) => option?.label || option}
                                value={selectedPart}
                                onChange={(event, newValue) => handlePartChange(newValue)}
                                required
                                renderInput={(params) => <TextField {...params} label="Part *" error={Boolean(validationErrors.part)} helperText={validationErrors.part}/>}
                                isOptionEqualToValue={(option, value) => true}
                            />
                        </FormControl>
                    </Grid>
                    <Grid item xs={12} sm={4}>
                        <FormControl fullWidth>
                            <Autocomplete
                                id="compatible-categories-autocomplete"
                                options={compatibleCategories.map(p => ({ id: p.id, label: p.id }))}
                                getOptionLabel={(option) => option?.label || option}
                                value={selectedCompCategory}
                                onChange={(event, newValue) => handleCompCategoryChange(newValue)}
                                required
                                renderInput={(params) => <TextField {...params} label="Compatible parts category *" error={Boolean(validationErrors.compCategory)} helperText={validationErrors.compCategory}/>}
                                isOptionEqualToValue={(option, value) => true}
                            />
                        </FormControl>
                    </Grid>
                </Grid>

                <Box component="form" noValidate onSubmit={(event) => getCompatibleParts(event)}>
                    <Button
                            type="submit"
                            fullWidth
                            variant="contained"
                            sx={{ maxWidth: '100%', mt: 4, mb: 2, fontWeight: 'bold', bgcolor: '#0d6267', '&:hover': { backgroundColor: '#07383b' } }}
                        >
                            GET COMPATIBLE PARTS
                    </Button>
                </Box>
            </Box>
            {compatibleParts !== null  && compatibleParts.length > 0 && (
                <Box sx={{mt: 3}}>
                    <Typography component="h1" variant="h5" sx={{ fontSize: '24px', fontWeight: 'bold', fontFamily: 'Arial, sans-serif', color: '#0d6267' }}>
                        COMPATIBLE PARTS
                    </Typography>
                    {compatibleParts.map((part) => (
                        <Link to={`${PATHS.AUCTIONS}?partId=${part.partId}`} key={part.partId} style={{ textDecoration: 'none' }}>
                            <Card style={{ margin: '10px', cursor: 'pointer', maxWidth: '100%', border: '1px solid #ddd'}} sx={{'&:hover': { boxShadow: '0 0 10px rgba(0, 0, 0, 1)' }}}>
                                <CardContent>
                                    <Typography variant="h5" component="h2" sx={{ fontSize: '20px', fontFamily: 'Arial, sans-serif', color: 'black' }}>
                                        {part.partName} ({part.activeAuctionForPartCnt})
                                    </Typography>
                                </CardContent>
                            </Card>
                        </Link>
                    ))}
                </Box>
            )}
        </Container>
    );
};

export default CompatibilityCheck;
