import { useContext, useEffect, useState } from "react";
import { getCategory } from "../../services/PartCategoryService";
import { Autocomplete, Box, Button, Container, CssBaseline, FormControl, Grid, InputLabel, MenuItem, Select, TextField, Typography } from "@mui/material";
import { checkTokenValidity, refreshAccessToken } from "../../services/AuthenticationService";
import { useNavigate, useParams } from "react-router-dom";
import { useUser } from "../../contexts/UserContext";
import SnackbarContext from "../../contexts/SnackbarContext";
import PATHS from "../../utils/Paths";
import { getPart, getParts, patchPart } from "../../services/PartService";
import { getAllCategorySeries } from "../../services/SeriesService";
import { patchAuction } from "../../services/AuctionService";
import { deletePartRequest, getPartRequest } from "../../services/PartRequestService";

function PartRequestsCreate() {
    const [categorySeries, setCategorySeries] = useState([]);
    const [partSeries, setPartSeries] = useState("none");
    const [categoryFields, setCategoryFields] = useState(null);
    const [name, setName] = useState("");
    const [specValue1, setSpecValue1] = useState("");
    const [specValue2, setSpecValue2] = useState("");
    const [specValue3, setSpecValue3] = useState("");
    const [specValue4, setSpecValue4] = useState("");
    const [specValue5, setSpecValue5] = useState("");
    const [specValue6, setSpecValue6] = useState("");
    const [specValue7, setSpecValue7] = useState("");
    const [specValue8, setSpecValue8] = useState("");
    const [specValue9, setSpecValue9] = useState("");
    const [specValue10, setSpecValue10] = useState("");
    const [partId, setPartId] = useState(null);
    const [auctionId, setAuctionId] = useState(null);

    const { requestId } = useParams();
    const { categoryId } = useParams();
    const [requestedPartName, setRequestedPartName] = useState("");
    const [part, setPart] = useState(null);
    const [categoryParts, setCategoryParts] = useState([]);

    const [validationErrors, setValidationErrors] = useState({
        name: null
    });

    const navigate = useNavigate();
    const { role, setLogin, setLogout } = useUser();
    const openSnackbar = useContext(SnackbarContext);

    const handlePartChange = async (part) => {
        setPart(part);
    }

    const handlePartSeriesChange = (e) =>
    {
        setPartSeries(e.target.value);
    };

    const handleNameChange = (e) => {
        setName(e.target.value);
    };

    const handleSpecValue1Change = (e) => {
        setSpecValue1(e.target.value);
    };

    const handleSpecValue2Change = (e) => {
        setSpecValue2(e.target.value);
    };

    const handleSpecValue3Change = (e) => {
        setSpecValue3(e.target.value);
    };

    const handleSpecValue4Change = (e) => {
        setSpecValue4(e.target.value);
    };

    const handleSpecValue5Change = (e) => {
        setSpecValue5(e.target.value);
    };

    const handleSpecValue6Change = (e) => {
        setSpecValue6(e.target.value);
    };

    const handleSpecValue7Change = (e) => {
        setSpecValue7(e.target.value);
    };

    const handleSpecValue8Change = (e) => {
        setSpecValue8(e.target.value);
    };
    const handleSpecValue9Change = (e) => {
        setSpecValue9(e.target.value);
    };

    const handleSpecValue10Change = (e) => {
        setSpecValue10(e.target.value);
    };
    const handleSubmit = async (e) => {
        e.preventDefault();

        let errors = [];
        setValidationErrors(errors);
        if(!(name && name.length >= 0) && part === null) errors.name = "Part name must be set"

        if (Object.keys(errors).length > 0) {
            setValidationErrors(errors);
            return;
        }

        const seriesId = partSeries === "none" ? null : partSeries;

        const patchData = {name, specificationValue1: specValue1, specificationValue2: specValue2, specificationValue3: specValue3,
            specificationValue4: specValue4, specificationValue5: specValue5, specificationValue6: specValue6,
            specificationValue7: specValue7,  specificationValue8: specValue8, specificationValue9: specValue9,
            specificationValue10: specValue10, seriesId, type: 'Permanent'};

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

        try {

            if(part)
            {
                const patchAuctionData = { categoryId: categoryId, partId: part.id }

                await patchAuction(patchAuctionData, auctionId);
                deletePartRequest(requestId);
                openSnackbar('Auction part updated successfully!', 'success');
                navigate(PATHS.PARTS);
            }

            if(part == null)
            {
                await patchPart(patchData, categoryId, partId);

                await patchAuction(null, auctionId);
                deletePartRequest(requestId);
                openSnackbar('New part created successfully!', 'success');
                navigate(PATHS.PARTS);
            }

        } catch(error) {
            openSnackbar(error.response.data.errorMessage, "error")
        }

    };

    useEffect(() => {
        if (!role.includes("Admin")) {
            openSnackbar('Only admins can access this page!', 'error');
            navigate(PATHS.MAIN);
        }

        const refreshToken = async () => {
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

            await fetchRequestData();
        };

        const fetchRequestData = async () => {
            try {
                const result = await getPartRequest(requestId);
                const fetchedPartId = result.partId;
                await setPartId(fetchedPartId);
                await setAuctionId(result.auctionId);

                await fetchCategoriesData();
                await fetchRequestedPartData(fetchedPartId);
                await fetchCategorySeries();
                await fetchCategoryParts();
            }
            catch(error)
            {
                openSnackbar('This part request does not exist!!', 'error');
                navigate(PATHS.PARTREQUESTS);
            }
        };

        const fetchCategoriesData = async () => {
            const result = await getCategory(categoryId);
            setCategoryFields(result);
        };

        const fetchRequestedPartData = async (fetchedPartId) => {
            const result = await getPart(categoryId, fetchedPartId);
            setRequestedPartName(result.name);
        };

        const fetchCategorySeries = async() => {
            const result = await getAllCategorySeries(categoryId);
            setCategorySeries(result);
        };

        const fetchCategoryParts = async () => {
            const result = await getParts(categoryId);
            setCategoryParts(result);
        }

        refreshToken();
    }, [navigate, openSnackbar, role, categoryId, partId, requestId, setLogin, setLogout]);

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
                <Typography component="h1" variant="h5" sx={{ fontSize: '26px', fontWeight: 'bold', fontFamily: 'Arial, sans-serif', color: '#0d6267' }}>
                    CREATE NEW PART
                </Typography>

                <Box component="form" noValidate onSubmit={(event) => handleSubmit(event)} sx={{ mt: 3 }}>
                    <Grid container spacing={2}>
                        <Grid item xs={12}>
                            <TextField
                                fullWidth
                                id="part-category"
                                label="Part category"
                                value={categoryId}
                                name="part-category"
                                disabled={true}
                                sx={{
                                    "& .MuiInputBase-input.Mui-disabled": {
                                        WebkitTextFillColor: "#138c94",
                                        fontWeight: 'bold'
                                    },
                                    "& .MuiInputBase-root.Mui-disabled": {
                                        "& > fieldset": {
                                         borderColor: "#138c94"
                                        }
                                    },
                                    '& label.Mui-disabled': {
                                        color: '#138c94'
                                    },
                                }}
                            />
                        </Grid>
                        <Grid item xs={12}>
                            <TextField
                                fullWidth
                                id="requestedPartName"
                                label="Requested part name"
                                value={requestedPartName}
                                disabled={true}
                                sx={{
                                    "& .MuiInputBase-input.Mui-disabled": {
                                        WebkitTextFillColor: "#138c94",
                                        fontWeight: 'bold'
                                    },
                                    "& .MuiInputBase-root.Mui-disabled": {
                                        "& > fieldset": {
                                         borderColor: "#138c94"
                                        }
                                    },
                                    '& label.Mui-disabled': {
                                        color: '#138c94'
                                    },
                                }}
                            />
                        </Grid>
                        <Grid item xs={12}>
                            <FormControl fullWidth>
                            <Autocomplete
                                id="part-autocomplete"
                                options={categoryParts.map(p => ({ id: p.id, label: p.name }))}
                                getOptionLabel={(option) => option?.label || option}
                                value={part}
                                onChange={(event, newValue) => {
                                    handlePartChange(newValue);
                                }}
                                required
                                renderInput={(params) => <TextField {...params} label="Part *" error={Boolean(validationErrors.part)} helperText={validationErrors.part}  />}
                                isOptionEqualToValue={(option, value) => { return true; }}
                            />
                            </FormControl>
                        </Grid>
                        <Grid item xs={12}>
                            <FormControl fullWidth>
                                <InputLabel id="part-category-label">Series</InputLabel>
                                <Select
                                    labelId="part-series-label"
                                    id="part-series"
                                    label="Series"
                                    value={partSeries}
                                    onChange={handlePartSeriesChange}
                                    required
                                    sx={{ textAlign: 'left' }}
                                >
                                    <MenuItem value = {"none"}>{"-"}</MenuItem>
                                    {categorySeries.map((s) => (
                                        <MenuItem key={s.id} value={s.id}>
                                            {s.name}
                                        </MenuItem>
                                    ))}
                                </Select>
                            </FormControl>
                        </Grid>
                        <Grid item xs={12}>
                            <TextField
                                error={Boolean(validationErrors.name)}
                                helperText={validationErrors.name}
                                required
                                fullWidth
                                id="name"
                                label="Name"
                                name="name"
                                autoFocus
                                onChange={handleNameChange}
                            />
                        </Grid>
                        {categoryFields && categoryFields.specificationName1 && (
                            <Grid item xs={12}>
                                <TextField
                                    fullWidth
                                    id="specValue1"
                                    label={categoryFields.specificationName1}
                                    name="specValue1"
                                    onChange={handleSpecValue1Change}
                                />
                             </Grid>
                        )}
                        {categoryFields && categoryFields.specificationName2 && (
                            <Grid item xs={12}>
                                <TextField
                                    fullWidth
                                    id="specValue2"
                                    label={categoryFields.specificationName2}
                                    name="specValue2"
                                    onChange={handleSpecValue2Change}
                                />
                            </Grid>
                        )}
                        {categoryFields && categoryFields.specificationName3 && (
                            <Grid item xs={12}>
                                <TextField
                                    fullWidth
                                    id="specValue3"
                                    label={categoryFields.specificationName3}
                                    name="specValue3"
                                    onChange={handleSpecValue3Change}
                                />
                            </Grid>
                        )}
                        {categoryFields && categoryFields.specificationName4 && (
                        <Grid item xs={12}>
                            <TextField
                                fullWidth
                                id="specValue4"
                                label={categoryFields.specificationName4}
                                name="specValue4"
                                onChange={handleSpecValue4Change}
                            />
                        </Grid>
                        )}
                        {categoryFields && categoryFields.specificationName5 && (
                        <Grid item xs={12}>
                            <TextField
                                fullWidth
                                id="specValue5"
                                label={categoryFields.specificationName5}
                                name="specValue5"
                                onChange={handleSpecValue5Change}
                            />
                        </Grid>
                        )}
                        {categoryFields && categoryFields.specificationName6 && (
                        <Grid item xs={12}>
                            <TextField
                                fullWidth
                                id="specValue6"
                                label={categoryFields.specificationName6}
                                name="specValue6"
                                onChange={handleSpecValue6Change}
                            />
                        </Grid>
                        )}
                        {categoryFields && categoryFields.specificationName7 && (
                        <Grid item xs={12}>
                            <TextField
                                fullWidth
                                id="specValue7"
                                label={categoryFields.specificationName7}
                                name="specValue7"
                                onChange={handleSpecValue7Change}
                            />
                        </Grid>
                        )}
                        {categoryFields && categoryFields.specificationName8 && (
                        <Grid item xs={12}>
                            <TextField
                                fullWidth
                                id="specValue8"
                                label={categoryFields.specificationName8}
                                name="specValue8"
                                onChange={handleSpecValue8Change}
                            />
                        </Grid>
                        )}
                        {categoryFields && categoryFields.specificationName9 && (
                        <Grid item xs={12}>
                            <TextField
                                fullWidth
                                id="specValue9"
                                label={categoryFields.specificationName9}
                                name="specValue9"
                                onChange={handleSpecValue9Change}
                            />
                        </Grid>
                        )}
                        {categoryFields && categoryFields.specificationName10 && (
                        <Grid item xs={12}>
                            <TextField
                                fullWidth
                                id="specValue10"
                                label={categoryFields.specificationName10}
                                name="specValue10"
                                onChange={handleSpecValue10Change}
                            />
                        </Grid>
                        )}
                    </Grid>
                    <Button
                        type="submit"
                        fullWidth
                        variant="contained"
                        sx={{ mt: 2, mb: 2, bgcolor: '#0d6267', '&:hover': { backgroundColor: '#07383b'}}}
                    >
                        CREATE PART
                    </Button>
                </Box>

            </Box>
        </Container>
    );
};

export default PartRequestsCreate;
