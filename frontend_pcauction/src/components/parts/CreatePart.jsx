import { useEffect, useState } from "react";
import { getCategories } from "../../services/PartCategoryService";
import { Box, Button, Container, CssBaseline, FormControl, Grid, InputLabel, MenuItem, Select, Typography } from "@mui/material";

function CreatePart() {
    const [categories, setCategories] = useState([]);
    const [partCategory, setPartCategory] = useState("");

    const handlePartCategoryChange = (e) => {
        setPartCategory(e.target.value);
    }

    const handleSubmit = async (e) => {
        e.preventDefault();

        console.log(categories);
    };

    useEffect(() => {
        const fetchCategoriesData = async () => {
            const result = await getCategories();
            setCategories(result);
        };

        fetchCategoriesData();

    }, []);

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
                <Typography component="h1" variant="h5" style={{ fontWeight: 'bold' }}>
                    CREATE NEW PART
                </Typography>

                <Box component="form" noValidate onSubmit={(event) => handleSubmit(event)} sx={{ mt: 3 }}>
                    <Grid container spacing={2}>
                        <Grid item xs={12}>
                            <FormControl fullWidth>
                                <InputLabel id="part-category-label">Part category *</InputLabel>
                                <Select
                                    labelId="part-category-label"
                                    id="part-category"
                                    label="Part category"
                                    value={partCategory}
                                    onChange={handlePartCategoryChange}
                                    required
                                    sx={{ textAlign: 'left' }}
                                >
                                    {categories.map((category) => (
                                        <MenuItem key={category.id} value={category.id}>
                                            {category.id}
                                        </MenuItem>
                                    ))}
                                </Select>
                            </FormControl>
                        </Grid>
                    </Grid>
                    <Button
                        type="submit"
                        fullWidth
                        variant="contained"
                        sx={{ mt: 1, mb: 2, bgcolor: '#0d6267' }}
                    >
                        CREATE PART
                    </Button>
                </Box>

            </Box>
        </Container>
    );
};

export default CreatePart;
