import { useEffect, useState } from "react";
import { getParts } from "../../services/PartService";
import { Autocomplete, Avatar, Box, Button, Card, CardActionArea, CardContent, CardHeader, Checkbox, Container, CssBaseline, FormControl, FormControlLabel, Grid, TextField, Typography } from "@mui/material";
import { generatePcBuild } from "../../services/CalculationsService";
import PATHS from "../../utils/Paths";
import { useNavigate } from "react-router-dom";
import { timeLeft } from "../../utils/DateUtils";

function PcBuildGenerator()
{
    const navigate = useNavigate();

    const [motherboards, setMotherboards] = useState([]);
    const [CPU, setCPU] = useState([]);
    const [GPU, setGPU] = useState([]);
    const [RAM, setRAM] = useState([]);
    const [SSD, setSSD] = useState([]);
    const [HDD, setHDD] = useState([]);
    const [PSU, setPSU] = useState([]);
    const [budget, setBudget] = useState(0);
    const [auctions, setAuctions] = useState([]);

    const [selectedMotherboard, setSelectedMotherboard] = useState(null);
    const [selectedCPU, setSelectedCPU] = useState(null);
    const [selectedGPU, setSelectedGPU] = useState(null);
    const [selectedRAM, setSelectedRAM] = useState(null);
    const [selectedSSD, setSelectedSSD] = useState(null);
    const [selectedHDD, setSelectedHDD] = useState(null);
    const [selectedPSU, setSelectedPSU] = useState(null);

    const [motherboardAlreadyHave, setMotherboardAlreadyHave] = useState(false);

    const motherboardAlreadyHaveChange = (event) => {
        setMotherboardAlreadyHave(event.target.checked);
      };


    const handleMotherboardChange = async (newValue) => {
        setSelectedMotherboard(newValue);
    }

    const handleCPUChange = async (newValue) => {
        setSelectedCPU(newValue);
    }

    const handleGPUChange = async (newValue) => {
        setSelectedGPU(newValue);
    }

    const handleRAMChange = async (newValue) => {
        setSelectedRAM(newValue);
    }

    const handleSSDChange = async (newValue) => {
        setSelectedSSD(newValue);
    }

    const handleHDDChange = async (newValue) => {
        setSelectedHDD(newValue);
    }

    const handlePSUChange = async (newValue) => {
        setSelectedPSU(newValue);
    }

    const handleBudgetChange = (event) => {
        setBudget(event.target.value);
    }

    useEffect(() => {

        const fetchMotherboards = async () => {
            const result = await getParts("Motherboard");
            setMotherboards([{ id: 'ANY', name: 'Any motherboard' }, ...result]);
        };

        const fetchRAM = async () => {
            const result = await getParts("RAM");
            setRAM([{ id: 'ANY', name: 'Any RAM' }, ...result]);
        };

        const fetchSSD = async () => {
            const result = await getParts("SSD");
            setSSD([{ id: 'ANY', name: 'Any SSD' }, ...result]);
        };

        const fetchHDD = async () => {
            const result = await getParts("HDD");
            setHDD([{ id: 'ANY', name: 'Any HDD' }, ...result]);
        };

        const fetchGPU = async () => {
            const result = await getParts("GPU");
            setGPU([{ id: 'ANY', name: 'Any GPU' }, ...result]);
        };

        const fetchCPU = async () => {
            const result = await getParts("CPU");
            setCPU([{ id: 'ANY', name: 'Any CPU' }, ...result]);
        };

        const fetchPSU = async () => {
            const result = await getParts("PSU");
            setPSU([{ id: 'ANY', name: 'Any PSU' }, ...result]);
        };

        fetchMotherboards();
        fetchCPU();
        fetchGPU();
        fetchRAM();
        fetchSSD();
        fetchHDD();
        fetchPSU();
    }, []);


    const generateBuild = async (e) => {
        e.preventDefault();

        const data= {
            motherboardId: selectedMotherboard ? selectedMotherboard.id : null, // TODO ADD VALIDATION THAT MOTHERBOARD CAN NOT BE NULL
            cpuId: selectedCPU ? selectedCPU.id : null,
            gpuId: selectedGPU ? selectedGPU.id : null,
            ramId: selectedRAM ? selectedRAM.id : null,
            hddId: selectedHDD ? selectedHDD.id : null,
            ssdId: selectedSSD ? selectedSSD.id : null,
            // TODO MAYBE ADD CHOOSING IF YOU NEED PSU (LIKE CHECKBOX)
            psuId: 'ANY',
            budget: budget
        };
        const result = await generatePcBuild(data);
        console.log(result.data);
        setAuctions(result.data);
    }

    const truncateText = (text, maxLength) => {
        if (text.length <= maxLength) return text;
        return text.slice(0, maxLength).trimEnd() + '...';
    };

    const handleCardClick = (auctionId) => {
        navigate(PATHS.AUCTIONINFO.replace(":auctionId", auctionId));
      };

    return (
        <Box>
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
                    PC BUILD GENERATOR
                </Typography>
                <Box component="form" noValidate onSubmit={(event) => generateBuild(event)} sx={{ mt: 3 }}>
                    <Grid container spacing={2}>
                        <Grid item xs={12}>
                            <FormControl fullWidth>
                                <Autocomplete
                                    id="motherboard-autocomplete"
                                    options={motherboards.map(p => ({ id: p.id, label: p.name }))}
                                    getOptionLabel={(option) => option?.label || option}
                                    value={selectedMotherboard}
                                    onChange={(event, newValue) => {
                                        handleMotherboardChange(newValue);
                                    }}
                                    required
                                    renderInput={(params) => <TextField {...params} label="Motherboard" />}
                                    isOptionEqualToValue={(option, value) => { return true; }}
                                    sx={{ width: '100%', '& input': { fontSize: '1rem' } }}
                                />
                            </FormControl>
                        </Grid>
                        <Grid item xs={12} container alignItems="center">
                            <FormControlLabel
                                control={<Checkbox checked={motherboardAlreadyHave} onChange={motherboardAlreadyHaveChange} />}
                                label={<Typography variant="body1" style={{ fontSize: '1.2rem', marginTop: 0 }}>Already have</Typography>}
                            />
                        </Grid>

                        <Grid item xs={12}>
                            <FormControl fullWidth>
                                <Autocomplete
                                    id="cpu-autocomplete"
                                    options={CPU.map(p => ({ id: p.id, label: p.name }))}
                                    getOptionLabel={(option) => option?.label || option}
                                    value={selectedCPU}
                                    onChange={(event, newValue) => {
                                        handleCPUChange(newValue);
                                    }}
                                    required
                                    renderInput={(params) => <TextField {...params} label="CPU" />}
                                    isOptionEqualToValue={(option, value) => { return true; }}
                                    sx={{ width: '100%', '& input': { fontSize: '1rem' } }}
                                />
                            </FormControl>
                        </Grid>
                        <Grid item xs={12}>
                            <FormControl fullWidth>
                                <Autocomplete
                                    id="gpu-autocomplete"
                                    options={GPU.map(p => ({ id: p.id, label: p.name }))}
                                    getOptionLabel={(option) => option?.label || option}
                                    value={selectedGPU}
                                    onChange={(event, newValue) => {
                                        handleGPUChange(newValue);
                                    }}
                                    required
                                    renderInput={(params) => <TextField {...params} label="GPU" />}
                                    isOptionEqualToValue={(option, value) => { return true; }}
                                    sx={{ width: '100%', '& input': { fontSize: '1rem' } }}
                                />
                            </FormControl>
                        </Grid>
                        <Grid item xs={12}>
                            <FormControl fullWidth>
                                <Autocomplete
                                    id="ram-autocomplete"
                                    options={RAM.map(p => ({ id: p.id, label: p.name }))}
                                    getOptionLabel={(option) => option?.label || option}
                                    value={selectedRAM}
                                    onChange={(event, newValue) => {
                                        handleRAMChange(newValue);
                                    }}
                                    required
                                    renderInput={(params) => <TextField {...params} label="RAM" />}
                                    isOptionEqualToValue={(option, value) => { return true; }}
                                    sx={{ width: '100%', '& input': { fontSize: '1rem' } }}
                                />
                            </FormControl>
                        </Grid>
                        <Grid item xs={12}>
                            <FormControl fullWidth>
                                <Autocomplete
                                    id="ssd-autocomplete"
                                    options={SSD.map(p => ({ id: p.id, label: p.name }))}
                                    getOptionLabel={(option) => option?.label || option}
                                    value={selectedSSD}
                                    onChange={(event, newValue) => {
                                        handleSSDChange(newValue);
                                    }}
                                    required
                                    renderInput={(params) => <TextField {...params} label="SSD" />}
                                    isOptionEqualToValue={(option, value) => { return true; }}
                                    sx={{ width: '100%', '& input': { fontSize: '1rem' } }}
                                />
                            </FormControl>
                        </Grid>
                        <Grid item xs={12}>
                            <FormControl fullWidth>
                                <Autocomplete
                                    id="hdd-autocomplete"
                                    options={HDD.map(p => ({ id: p.id, label: p.name }))}
                                    getOptionLabel={(option) => option?.label || option}
                                    value={selectedHDD}
                                    onChange={(event, newValue) => {
                                        handleHDDChange(newValue);
                                    }}
                                    required
                                    renderInput={(params) => <TextField {...params} label="HDD" />}
                                    isOptionEqualToValue={(option, value) => { return true; }}
                                    sx={{ width: '100%', '& input': { fontSize: '1rem' } }}
                                />
                            </FormControl>
                        </Grid>
                        <Grid item xs={12}>
                            <TextField
                                required
                                fullWidth
                                id="budget"
                                label="Budget"
                                name="budget"
                                type="number"
                                inputProps={{ min: 0 }}
                                value = {budget}
                                onChange = { handleBudgetChange }

                            />
                        </Grid>
                    </Grid>
                    <Button
                        type="submit"
                        fullWidth
                        variant="contained"
                        sx={{ mt: 2, mb: 2, fontWeight: 'bold', bgcolor: '#0d6267', '&:hover': { backgroundColor: '#07383b'} }}
                    >
                        GENERATE
                    </Button>
                </Box>
            </Box>
        </Container>
        <Container component="main" maxWidth="md">
        {auctions.length ? (
        <Box
            sx={{
            marginTop: 8,
            padding: '20px',
            }}
        >
            <Box sx={{ marginTop: 2, marginBottom: 3, display: 'flex', flexDirection: 'column', alignItems: 'center' }}>
            <Typography component="h1" variant="h5" sx={{ fontSize: '26px', fontWeight: 'bold', fontFamily: 'Arial, sans-serif', color: '#0d6267' }}>
                PC BUILD
            </Typography>
            </Box>
            <Box>
            {auctions.map((auction, index) => (
                <Card key = {auction.id}  display="flex" sx={{ marginBottom: 2, border: '1px solid #ddd' }}>
                <CardActionArea onClick={() => handleCardClick(auction.id)} sx={{ width: '100%', display: 'flex', '&:hover': { boxShadow: '0 0 10px rgba(0, 0, 0, 1)' } }}>
                <Box sx={{ flexGrow: 1 }}>
                    <CardHeader
                    title={<Typography variant="h6" sx ={{fontFamily: 'Arial, sans-serif', fontWeight: 'bold', fontSize: '22px', color: '#0d6267'}}>{auction.name}</Typography>}
                    sx={{
                        textAlign: 'left',
                        wordBreak: 'break-all',
                        overflow: 'hidden',
                        paddingBottom: 0
                    }}
                    />
                    <CardContent>
                    <Typography
                        variant="subtitle1"
                        sx={{
                        textAlign: 'left',
                        fontWeight: 'bold',
                        color: '#255e62',
                        fontFamily: 'Arial, sans-serif',
                        }}
                    >
                        {auction.partName}, {auction.categoryId}&nbsp;
                    </Typography>
                    <Box sx = {{textAlign: 'left',}}>
                        <Typography component="span" variant="subtitle1" sx={{ fontWeight: 'bold', color: '#255e62', fontFamily: 'Arial, sans-serif', display: 'inline-block'}}>
                            Time left:&nbsp;
                        </Typography>
                        <Typography component="span" sx={{fontFamily: 'Arial, sans-serif', fontWeight: 'bold', color: '#c21818', display: 'inline-block'}}>
                        {timeLeft(auction.endDate, new Date().toISOString().slice(0, 19))}
                        </Typography>
                    </Box>
                    <Typography
                        variant="body2"
                        color="textSecondary"
                        component="p"
                        sx={{
                        textAlign: 'left',
                        fontFamily: 'Arial, sans-serif',
                        wordBreak: 'break-all',
                        overflow: 'hidden',
                        color: 'black'
                        }}
                    >
                        {truncateText(auction.description, 180)}
                    </Typography>
                    </CardContent>
                </Box>
                <Avatar
                    alt="Auction Image"
                    src={auction.imageUri}
                    sx={{
                    width: '200px',
                    height: '200px',
                    borderRadius: '0'
                    }}
                />
                </CardActionArea>
                </Card>
            ))}
            </Box>
        </Box>
        ) : (
            null
        )}
        </Container>
        </Box>
    );
}

export default PcBuildGenerator;
