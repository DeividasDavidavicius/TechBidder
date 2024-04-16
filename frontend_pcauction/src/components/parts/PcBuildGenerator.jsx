import { useEffect, useState } from "react";
import { getParts } from "../../services/PartService";
import { Autocomplete, Avatar, Box, Button, Card, CardActionArea, CardContent, CardHeader, Checkbox, Container, CssBaseline, FormControl, FormControlLabel, Grid, TextField, Typography } from "@mui/material";
import { generatePcBuild } from "../../services/CalculationsService";
import PATHS from "../../utils/Paths";
import { timeLeft } from "../../utils/DateUtils";
import { getHighestBid } from "../../services/BidService";
import { Link } from "react-router-dom";

function PcBuildGenerator()
{
    const [motherboards, setMotherboards] = useState([]);
    const [CPU, setCPU] = useState([]);
    const [GPU, setGPU] = useState([]);
    const [RAM, setRAM] = useState([]);
    const [SSD, setSSD] = useState([]);
    const [HDD, setHDD] = useState([]);
    const [budget, setBudget] = useState(0);
    const [auctions, setAuctions] = useState([]);

    const [selectedMotherboard, setSelectedMotherboard] = useState(null);
    const [selectedCPU, setSelectedCPU] = useState(null);
    const [selectedGPU, setSelectedGPU] = useState(null);
    const [selectedRAM, setSelectedRAM] = useState(null);
    const [selectedSSD, setSelectedSSD] = useState(null);
    const [selectedHDD, setSelectedHDD] = useState(null);

    const [motherboardAlreadyHave, setMotherboardAlreadyHave] = useState(false);
    const [cpuAlreadyHave, setCpuAlreadyHave] = useState(false);
    const [gpuAlreadyHave, setGpuAlreadyHave] = useState(false);
    const [ramAlreadyHave, setRamAlreadyHave] = useState(false);
    const [ssdAlreadyHave, setSsdAlreadyHave] = useState(false);
    const [hddAlreadyHave, setHddAlreadyHave] = useState(false);
    const [includePsu, setIncludePsu] = useState(true);

    const [totalPrice, setTotalPrice] = useState(0);

    const [message, setMessage] = useState(null);
    const [validationErrors, setValidationErrors] = useState({
        motherboard: null,
        cpu: null,
        gpu: null,
        ram: null,
        ssd: null,
        hdd: null
    });

    const handleMotherboardAlreadyHaveChange = (event) => {
        setMotherboardAlreadyHave(event.target.checked);
        const errors = validationErrors;
        errors.motherboard = null;
        setValidationErrors(errors);
    };

    const handleCpuAlreadyHaveChange = (event) => {
        setCpuAlreadyHave(event.target.checked);
        const errors = validationErrors;
        errors.CPU = null;
        setValidationErrors(errors);
    };

    const handleGpuAlreadyHaveChange = (event) => {
        setGpuAlreadyHave(event.target.checked);
        const errors = validationErrors;
        errors.GPU = null;
        setValidationErrors(errors);
    };

    const handleRamAlreadyHaveChange = (event) => {
        setRamAlreadyHave(event.target.checked);
        const errors = validationErrors;
        errors.RAM = null;
        setValidationErrors(errors);
    };

    const handleSsdAlreadyHaveChange = (event) => {
        setSsdAlreadyHave(event.target.checked);
        const errors = validationErrors;
        errors.SSD = null;
        setValidationErrors(errors);
    };

    const handleHddAlreadyHaveChange = (event) => {
        setHddAlreadyHave(event.target.checked);
        const errors = validationErrors;
        errors.HDD = null;
        setValidationErrors(errors);
    }

    const handleIncludePsuChange = (event) => {
        setIncludePsu(event.target.checked);
    }


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

        fetchMotherboards();
        fetchCPU();
        fetchGPU();
        fetchRAM();
        fetchSSD();
        fetchHDD();
    }, []);

    const generateBuild = async (e) => {
        e.preventDefault();

        let errors = [];
        setValidationErrors(errors);

        if(motherboardAlreadyHave === false && selectedMotherboard === null) errors.motherboard = 'Select motherboard';
        if(motherboardAlreadyHave && (selectedMotherboard === null || selectedMotherboard.id === 'ANY')) errors.motherboard = 'Select specific motherboard';
        if(cpuAlreadyHave && (selectedCPU === null || selectedCPU.id === 'ANY')) errors.CPU = 'Select specific CPU';
        if(gpuAlreadyHave && (selectedGPU === null || selectedGPU.id === 'ANY')) errors.GPU = 'Select specific GPU';
        if(ramAlreadyHave && (selectedRAM === null || selectedRAM.id === 'ANY')) errors.RAM = 'Select specific RAM';
        if(ssdAlreadyHave && (selectedSSD === null || selectedSSD.id === 'ANY')) errors.SSD = 'Select specific SSD';
        if(hddAlreadyHave && (selectedHDD === null || selectedHDD.id === 'ANY')) errors.HDD = 'Select specific HDD';

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
            ssdId: selectedSSD ? selectedSSD.id : null,
            psuId: 'ANY',
            motherboardAlreadyHave,
            cpuAlreadyHave,
            gpuAlreadyHave,
            ramAlreadyHave,
            ssdAlreadyHave,
            hddAlreadyHave,
            includePsu,
            budget: budget ? budget : 0
        };

        try {
            const result = await generatePcBuild(data);

            setTotalPrice(result.reduce((acc, curr) => acc + curr.averagePrice, 0));

            const auctionsWithHighestBid = await Promise.all(
                (result).map(async (auction) => {
                  const highestBid = await getHighestBid(auction.id);
                  const highestBidAmount = highestBid.amount;
                  return { ...auction, highestBidAmount };
                })
              );

            setAuctions(auctionsWithHighestBid);
            setMessage(null);
        }
        catch(error)
        {
            setAuctions([]);
            setMessage(error.response.data);
        }
    }

    const truncateText = (text, maxLength) => {
        if (text.length <= maxLength) return text;
        return text.slice(0, maxLength).trimEnd() + '...';
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
                        {[
                            { label: "Motherboard", id: "motherboard-autocomplete", options: motherboards, value: selectedMotherboard, onChange: handleMotherboardChange, checked: motherboardAlreadyHave, onChangeChecked: handleMotherboardAlreadyHaveChange, error: validationErrors.motherboard },
                            { label: "CPU", id: "cpu-autocomplete", options: CPU, value: selectedCPU, onChange: handleCPUChange, checked: cpuAlreadyHave, onChangeChecked: handleCpuAlreadyHaveChange, error: validationErrors.CPU },
                            { label: "GPU", id: "gpu-autocomplete", options: GPU, value: selectedGPU, onChange: handleGPUChange, checked: gpuAlreadyHave, onChangeChecked: handleGpuAlreadyHaveChange, error: validationErrors.GPU },
                            { label: "RAM", id: "ram-autocomplete", options: RAM, value: selectedRAM, onChange: handleRAMChange, checked: ramAlreadyHave, onChangeChecked: handleRamAlreadyHaveChange, error: validationErrors.RAM },
                            { label: "SSD", id: "ssd-autocomplete", options: SSD, value: selectedSSD, onChange: handleSSDChange, checked: ssdAlreadyHave, onChangeChecked: handleSsdAlreadyHaveChange, error: validationErrors.SSD },
                            { label: "HDD", id: "hdd-autocomplete", options: HDD, value: selectedHDD, onChange: handleHDDChange, checked: hddAlreadyHave, onChangeChecked: handleHddAlreadyHaveChange, error: validationErrors.HDD },
                        ].map((item, index) => (
                            <Grid item xs={12} key={index} sx={{ display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
                                <FormControl fullWidth sx={{ flexGrow: 1 }}>
                                    <Autocomplete
                                        id={item.id}
                                        options={item.options.map(p => ({ id: p.id, label: p.name }))}
                                        getOptionLabel={(option) => option?.label || option}
                                        value={item.value}
                                        onChange={(event, newValue) => {
                                            item.onChange(newValue);
                                        }}
                                        required
                                        renderInput={(params) => <TextField {...params} label={item.label} error={Boolean(item.error)} helperText={item.error}/>}
                                        isOptionEqualToValue={(option, value) => { return true; }}
                                        sx={{ width: '100%', '& input': { fontSize: '1rem' } }}
                                    />
                                </FormControl>
                                <FormControlLabel
                                    control={<Checkbox checked={item.checked} onChange={item.onChangeChecked} />}
                                    label={<Typography variant="body1" style={{ fontSize: '1.2rem', marginTop: 0, whiteSpace: 'nowrap' }}>Already have</Typography>}
                                    sx={{ ml: 2 }}
                                />
                            </Grid>
                        ))}
                        <Grid item xs={12}>
                            <TextField
                                required
                                fullWidth
                                id="budget"
                                label="Budget"
                                name="budget"
                                type="number"
                                inputProps={{ min: 0 }}
                                value={budget}
                                onChange={handleBudgetChange}
                            />
                        </Grid>
                        <FormControlLabel
                                    control={<Checkbox checked={includePsu} onChange={handleIncludePsuChange} />}
                                    label={<Typography variant="body1" style={{ fontSize: '1.2rem', marginTop: 0, whiteSpace: 'nowrap' }}>Include PSU</Typography>}
                                    sx={{ ml: 1, marginTop: 2 }}
                        />
                    </Grid>
                    <Button
                        type="submit"
                        fullWidth
                        variant="contained"
                        sx={{ mt: 2, mb: 2, fontWeight: 'bold', bgcolor: '#0d6267', '&:hover': { backgroundColor: '#07383b' } }}
                    >
                        GENERATE
                    </Button>
                </Box>
            </Box>
        </Container>
        <Container component="main" maxWidth="lg">

        {message !== null && (
        <Typography component="h1" variant="h5" sx={{ fontSize: '26px', fontWeight: 'bold', fontFamily: 'Arial, sans-serif', color: '#0d6267' }}>
            {message}
        </Typography>
        )}
        {auctions.length ? (
        <Box
            sx={{
            marginTop: 4,
            padding: '20px',
            }}
        >
            <Box sx={{ marginTop: 2, marginBottom: 3, display: 'flex', flexDirection: 'column', alignItems: 'center' }}>
                <Typography component="h1" variant="h5" sx={{ fontSize: '26px', fontWeight: 'bold', fontFamily: 'Arial, sans-serif', color: '#0d6267' }}>
                    PC BUILD
                </Typography>
                <Typography component="h1" variant="h5" sx={{ fontSize: '18px', fontWeight: 'bold', fontFamily: 'Arial, sans-serif', color: '#0d6267' }}>
                    (AVERAGE BUILD PRICE = {totalPrice}€)
                </Typography>
            </Box>
            <Box>
            {auctions.map((auction, index) => (
                <Card key = {auction.id}  display="flex" sx={{ marginBottom: 2, border: '1px solid #ddd' }}>
                    <Link to={PATHS.AUCTIONINFO.replace(":auctionId", auction.id)} style={{ textDecoration: 'none', color: 'inherit' }}>
                        <CardActionArea sx={{ width: '100%', display: 'flex', '&:hover': { boxShadow: '0 0 10px rgba(0, 0, 0, 1)' } }}>
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
                                {auction.highestBidAmount > 0  && (
                                <>
                                <Box sx = {{textAlign: 'left',}}>
                                <Typography component="span" variant="subtitle1" sx={{ fontWeight: 'bold', fontSize: '20px', color: '#255e62', fontFamily: 'Arial, sans-serif', display: 'inline-block'}}>
                                    Highest bid:&nbsp;
                                </Typography>
                                <Typography component="span" sx={{fontFamily: 'Arial, sans-serif', fontWeight: 'bold', fontSize: '20px', color: '#c21818', display: 'inline-block'}}>
                                    {auction.highestBidAmount}
                                </Typography>
                                </Box>
                                </> )}
                                <Box sx = {{textAlign: 'left',}}>
                                <Typography component="span" variant="subtitle1" sx={{ fontWeight: 'bold', fontSize: '20px', color: '#255e62', fontFamily: 'Arial, sans-serif', display: 'inline-block'}}>
                                    Average price:&nbsp;
                                </Typography>
                                <Typography component="span" sx={{fontFamily: 'Arial, sans-serif', fontWeight: 'bold', fontSize: '20px', color: '#c21818', display: 'inline-block'}}>
                                    {auction.averagePrice}
                                </Typography>
                                </Box>
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
                    </Link>
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
