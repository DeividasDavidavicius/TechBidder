import { useEffect, useState } from "react";
import { getParts } from "../../services/PartService";
import { Autocomplete, Box, Button, Checkbox, Container, CssBaseline, FormControl, FormControlLabel, Grid, TextField, Typography } from "@mui/material";
import { generatePcBuild } from "../../services/CalculationsService";

function PcBuildGenerator()
{
    const [motherboards, setMotherboards] = useState([]);
    const [CPU, setCPU] = useState([]);
    const [GPU, setGPU] = useState([]);
    const [RAM, setRAM] = useState([]);
    const [SSD, setSSD] = useState([]);
    const [HDD, setHDD] = useState([]);
    const [PSU, setPSU] = useState([]);

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
            psuId: selectedPSU ? selectedPSU.id : null // MAYBE REMOVE PSU COMPLETELY AND ONLY ADD IT IN CALCULATIONS
        };

        await generatePcBuild(data);
    }

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
                            <FormControl fullWidth>
                                <Autocomplete
                                    id="psu-autocomplete"
                                    options={PSU.map(p => ({ id: p.id, label: p.name }))}
                                    getOptionLabel={(option) => option?.label || option}
                                    value={selectedPSU}
                                    onChange={(event, newValue) => {
                                        handlePSUChange(newValue);
                                    }}
                                    required
                                    renderInput={(params) => <TextField {...params} label="PSU" />}
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
                        GENERATE
                    </Button>
                </Box>
            </Box>
        </Container>
    );
}

export default PcBuildGenerator;
