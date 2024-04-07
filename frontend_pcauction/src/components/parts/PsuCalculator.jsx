import { Autocomplete, Box, Button, Container, CssBaseline, FormControl, Grid, TextField, Typography } from "@mui/material";
import { useEffect, useState } from "react";
import { getParts } from "../../services/PartService";
import { calculatePsu } from "../../services/CalculationsService";

function PsuCalculator() {

    const [motherboards, setMotherboards] = useState([]);
    const [CPU, setCPU] = useState([]);
    const [GPU, setGPU] = useState([]);
    const [RAM, setRAM] = useState([]);
    const [SSD, setSSD] = useState([]);
    const [HDD, setHDD] = useState([]);

    const [selectedMotherboard, setSelectedMotherboard] = useState(null);
    const [selectedCPU, setSelectedCPU] = useState(null);
    const [selectedGPU, setSelectedGPU] = useState(null);
    const [selectedRAM, setSelectedRAM] = useState(null);
    const [selectedSSD, setSelectedSSD] = useState(null);
    const [selectedHDD, setSelectedHDD] = useState(null);

    const [validationErrors, setValidationErrors] = useState({
        motherboard: null,
        cpu: null,
        gpu: null,
        ram: null
    });

    const [result, setResult] = useState(null);

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

    const getPsuSuggestions = async (e) => {
        e.preventDefault();

        console.log(selectedCPU);

        let errors = [];
        setValidationErrors(errors);
        if(selectedCPU == null) errors.cpu = "Select CPU";
        if(selectedGPU == null) errors.gpu = "Select GPU";
        if(selectedRAM == null) errors.ram = "Select RAM";
        if(selectedMotherboard == null) errors.motherboard = "Select motherboard";

        if (Object.keys(errors).length > 0) {
            setValidationErrors(errors);
            return;
        }

        const data= {
            motherboardId: selectedMotherboard ? selectedMotherboard.id : null,
            cpuId: selectedCPU ? selectedCPU.id : null,
            gpuId: selectedGPU ? selectedGPU.id : null,
            ramId: selectedRAM ? selectedRAM.id : null,
            hddId: selectedHDD ? selectedHDD.id : null,
            ssdId: selectedSSD ? selectedSSD.id : null
        };

        const result = await calculatePsu(data);
        setResult(result.data);
    }

    useEffect(() => {

        const fetchMotherboards = async () => {
            const result = await getParts("Motherboard");
            setMotherboards(result);
        };

        const fetchRAM = async () => {
            const result = await getParts("RAM");
            setRAM(result);
        };

        const fetchSSD = async () => {
            const result = await getParts("SSD");
            setSSD(result);
        };

        const fetchHDD = async () => {
            const result = await getParts("HDD");
            setHDD(result);
        };

        const fetchGPU = async () => {
            const result = await getParts("GPU");
            setGPU(result);
        };

        const fetchCPU = async () => {
            const result = await getParts("CPU");
            setCPU(result);
        };

        fetchMotherboards();
        fetchCPU();
        fetchGPU();
        fetchRAM();
        fetchSSD();
        fetchHDD();
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
                <Typography component="h1" variant="h5" sx={{ fontSize: '26px', fontWeight: 'bold', fontFamily: 'Arial, sans-serif', color: '#0d6267' }}>
                    PSU CALCULATOR
                </Typography>
                <Box component="form" noValidate onSubmit={(event) => getPsuSuggestions(event)} sx={{ mt: 3 }}>
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
                                    renderInput={(params) => <TextField {...params} label="Motherboard *" error={Boolean(validationErrors.motherboard)} helperText={validationErrors.motherboard} />}
                                    isOptionEqualToValue={(option, value) => { return true; }}
                                    sx={{ width: '100%', '& input': { fontSize: '1rem' } }}
                                />
                            </FormControl>
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
                                    renderInput={(params) => <TextField {...params} label="CPU *" error={Boolean(validationErrors.cpu)} helperText={validationErrors.cpu}/>}
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
                                    renderInput={(params) => <TextField {...params} label="GPU *" error={Boolean(validationErrors.gpu)} helperText={validationErrors.gpu} />}
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
                                    renderInput={(params) => <TextField {...params} label="RAM" error={Boolean(validationErrors.ram)} helperText={validationErrors.ram} />}
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
                    </Grid>
                    <Button
                        type="submit"
                        fullWidth
                        variant="contained"
                        sx={{ mt: 2, mb: 2, fontWeight: 'bold', bgcolor: '#0d6267', '&:hover': { backgroundColor: '#07383b'} }}
                    >
                        CALCULATE
                    </Button>
                </Box>
                {result &&
                    <>
                        <Typography component="h1" variant="h5" sx={{ fontSize: '20px', fontWeight: 'bold', fontFamily: 'Arial, sans-serif', color: '#0d6267' }}>
                            Calculated PSU size: {result.calculatedWattage}W
                        </Typography>
                        {result.recommendedWattage !== -1 &&
                            <>
                                <Typography component="h1" variant="h5" sx={{ fontSize: '20px', fontWeight: 'bold', fontFamily: 'Arial, sans-serif', color: '#0d6267' }}>
                                    Recommended PSU size: {result.recommendedWattage}W
                                </Typography>
                            </>
                        }
                    </>
                }
            </Box>
        </Container>
    );

}

export default PsuCalculator;
