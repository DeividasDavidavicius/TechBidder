import { useContext, useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { getAuction } from "../../services/AuctionService";
import SnackbarContext from "../../contexts/SnackbarContext";
import { Avatar, Box, Button, Container, CssBaseline, Grid, TextField, Typography } from "@mui/material";
import PATHS from "../../utils/Paths";
import { checkTokenValidity, refreshAccessToken } from "../../services/AuthenticationService";
import { useUser } from "../../contexts/UserContext";
import { getHighestBid, postBid } from "../../services/BIdService";
import { getCategory } from "../../services/PartCategoryService";
import { getPart } from "../../services/PartService";
import { getSeries } from "../../services/SeriesService";
import CountdownTimer from "./CountdownTimer";
import AuctionRecommendations from "./AuctionRecommendations";

function AuctionInfo() {
    const [auctionData, setAuctionData] = useState({});
    const { auctionId } = useParams();
    const [startDateLocal, setStartDateLocal] = useState(null);
    const [endDateLocal, setEndDateLocal] = useState(null);
    const [minIncrement, setMinIncrement] = useState(0);
    const [highestBid, setHighestBid] = useState(0);
    const [imageUri, setImageUri] = useState(null);
    const openSnackbar = useContext(SnackbarContext);
    const navigate = useNavigate();
    const { setLogin, setLogout } = useUser();
    const [bidAmountField, setBidAmountField] = useState(0);

    const [partData, setPartData] = useState([]);
    const [categoryData, setCategoryData] = useState([]);
    const [seriesData, setSeriesData] = useState([]);

    const handleBidChange = (event) => {
        setBidAmountField(event.target.value);
      };

    const handleSubmitBid = async (e) => {
        e.preventDefault();

        const accessToken = localStorage.getItem('accessToken');
        if (!checkTokenValidity(accessToken)) {
            const result = await refreshAccessToken();
            if (!result.success) {
                openSnackbar('You must login to place bids!', 'error');
                setLogout();
                navigate(PATHS.LOGIN);
                return;
            }

            setLogin(result.response.data.accessToken, result.response.data.refreshToken);
        }

        const bidAmount = e.target.bid.value;

        let errors = [];
        const highestBidResult = await getHighestBid(auctionId);
        const highestBid = highestBidResult.amount;

        if (highestBid !== -1)
        {
            if (minIncrement > 0 && bidAmount < highestBid + minIncrement)
            {
                errors.bidAmount = "Your bid is not higher by min. increment than previous bid";
            }

            if (minIncrement === 0 && bidAmount <= highestBid)
            {
                errors.bidAmount = "Your bid is not higher than the previous bid";
            }
        }
        else if (minIncrement > 0 && bidAmount < minIncrement)
        {
            errors.bidAmount = "Starting bid must be equal or higher than min. increment";
        }
        else if (minIncrement === 0 && bidAmount <= 0)
        {
            errors.bidAmount = "Starting bid must be a positive number";
        }

        if(bidAmount > 50000)
        {
            errors.bidAmount = "Bid can not exceed 50000€";
        }

        if (Object.keys(errors).length > 0) {
            openSnackbar(errors.bidAmount, 'error');
            return;
        }

        const data = {amount: bidAmount};
        await postBid(data, auctionId);

        openSnackbar('Bid placed successfully!', 'success');
        setBidAmountField(0);
        setHighestBid(bidAmount);
        //window.location.reload();
        // navigate(PATHS.AUCTIONINFO.replace(":auctionId", auctionId));
    };

    useEffect(() => {
        const fetchCategoryData = async (categoryId) => {
            const result = await getCategory(categoryId);
            setCategoryData(result);
        };

        const fetchPartData = async (categoryId, partId) => {
            const result = await getPart(categoryId, partId);
            setPartData(result);

            if(result.seriesId != null)
            {
                fetchSeriesData(categoryId, result.seriesId);
            }
        };

        const fetchSeriesData = async(categoryId, seriesId) => {
            const result = await getSeries(categoryId, seriesId);
            setSeriesData(result);
        };

        const fetchAuctionData = async () => {
            try {
                const result = await getAuction(auctionId);
                setAuctionData(result);

                const offsetInMilliseconds = new Date().getTimezoneOffset() * 60000;
                const utcDateStart = new Date(result.startDate);
                const utcDateEnd = new Date(result.endDate);

                const localDateStart = new Date(utcDateStart.getTime() - offsetInMilliseconds).toLocaleString();
                const localDateEnd = new Date(utcDateEnd.getTime() - offsetInMilliseconds).toLocaleString();

                setImageUri(result.imageUri);
                setStartDateLocal(localDateStart);
                setEndDateLocal(localDateEnd);
                setMinIncrement(result.minIncrement);

                await fetchCategoryData(result.categoryId);
                await fetchPartData(result.categoryId, result.partId)
            }
            catch {
                openSnackbar('This auction does not exist!', 'error');
                //avigate(PATHS.MAIN);
            }
        };

        const fetchHighestBidData = async () => {
            try {
                const result = await getHighestBid(auctionId);
                setHighestBid(result.amount);
            }
            catch {
                openSnackbar('This auction does not exist!', 'error');
                navigate(PATHS.MAIN);
            }
        };

        fetchAuctionData();
        fetchHighestBidData();
      }, [auctionId, navigate, openSnackbar]);

      return (
        <Container component="main" maxWidth="lg">
            <CssBaseline />
            <Box
                sx={{
                marginTop: 8,
                border: '1px solid #ccc',
                borderRadius: '10px',
                }}
            >
                <Box sx={{ marginTop: 2, display: 'flex', flexDirection: 'column', alignItems: 'center' }}>
                    <Typography component="h1" variant="h5" sx={{ fontSize: '26px', fontWeight: 'bold', fontFamily: 'Arial, sans-serif', color: '#0d6267' }}>
                        {auctionData.name}
                    </Typography>
                </Box>

                <Box
                    sx={{
                        display: 'flex',
                        flexDirection: { xs: 'column',sm: 'column', md: 'row', lg: 'row' },
                        alignItems: 'center',
                        justifyContent: 'center',
                        textAlign: 'center',
                        marginTop: 2
                    }}
                >
                    <Box
                        sx={{
                        width: { sm: '90', md:'50%' },
                        display: 'flex',
                        justifyContent: 'center',
                        marginLeft: { xs: 3, sm: 3, md: 3, lg: 3 },
                        marginRight: { xs: 3, sm: 3, md: 3, lg: 3 },
                        marginBottom: 1
                        }}
                    >
                        <Avatar
                        alt="User Photo"
                            src={imageUri}
                        sx={{
                            width: '100%',
                            height: '90%',
                            borderRadius: '0'
                        }}
                        />
                    </Box>
                    <Box
                        component="form" noValidate onSubmit={(event) => handleSubmitBid(event)}
                        sx={{
                        width: { xs: '90%', sm: '90%', md: '50%', lg: '50%' },
                        display: 'flex',
                        flexDirection: 'column',
                        alignItems: 'left',
                        textAlign: 'left',
                        marginLeft: { xs: 3, sm: 3, md: 3, lg: 3 },
                        marginRight: { xs: 3, sm: 3, md: 3, lg: 3 },
                        marginBottom: 1
                        }}
                    >
                        <Grid item xs={12}>
                            <Box sx={{ display: 'flex', alignItems: 'stretch' }}>
                                <Typography
                                    component="span"
                                    variant="subtitle1"
                                    sx={{
                                        fontWeight: 'bold',
                                        fontSize: '36px',
                                        fontFamily: 'Arial, sans-serif',
                                        textTransform: 'uppercase',
                                        color: '#3b9298',
                                        lineHeight: 1}}
                                    >
                                        Highest bid:
                                </Typography >
                                <Typography
                                    component="span"
                                    variant="subtitle1"
                                    sx={{
                                        fontWeight: 'bold',
                                        fontSize: '36px',
                                        fontFamily: 'Arial, sans-serif',
                                        color: '#333',
                                        letterSpacing: '1px',
                                        textTransform: 'uppercase',
                                        lineHeight: 1}}
                                    >
                                        &nbsp;{highestBid === -1 ? "NONE" : highestBid + "€"}
                                </Typography >
                            </Box>
                        </Grid>
                        <Grid item xs={12}>
                            <Box sx={{ display: 'flex', alignItems: 'stretch', marginTop: 1 }}>
                                <Typography
                                    component="span"
                                    variant="subtitle1"
                                    sx={{
                                        fontWeight: 'bold',
                                        fontSize: '16px',
                                        fontFamily: 'Arial, sans-serif',
                                        letterSpacing: '1px',
                                        textTransform: 'uppercase',
                                        color: '#3b9298' }}
                                >
                                    Min. increment:
                                </Typography >
                                <Typography
                                    component="span"
                                    variant="subtitle1"
                                    sx={{
                                        fontWeight: 'bold',
                                        fontSize: '16px',
                                        fontFamily: 'Arial, sans-serif',
                                        color: '#333',
                                        letterSpacing: '1px' }}
                                >
                                &nbsp;{minIncrement}
                                </Typography >
                            </Box>
                        </Grid>
                        <Grid item xs={12} sx={{ marginTop: 1 }}>
                            <Box sx={{ display: 'flex', alignItems: 'stretch' }}>
                                <TextField
                                    required
                                    fullWidth
                                    id="bid"
                                    label="Bid amount"
                                    name="bid"
                                    type="number"
                                    inputProps={{ min: 0 }}
                                    value={bidAmountField}
                                    onChange={handleBidChange}
                                />
                                <Button
                                    type="submit"
                                    variant="contained"
                                    color="primary"
                                    sx={{
                                        width: '50%',
                                        borderTopLeftRadius: '10px',
                                        borderBottomLeftRadius: '10px',
                                        borderTopRightRadius: '10px',
                                        borderBottomRightRadius: '10px',
                                        marginLeft: 3,
                                        bgcolor: '#0d6267',
                                        fontSize: '14px',
                                        fontWeight: 'bold',
                                        '&:hover': {
                                            backgroundColor: '#3d8185',
                                          },
                                    }}
                                >
                                    PLACE BID
                                </Button>
                            </Box>
                        </Grid>
                        <Grid item xs={12}>
                            <Box sx={{ display: 'flex', alignItems: 'stretch', marginTop: 2 }}>
                                <Typography
                                    component="span"
                                    variant="subtitle1"
                                    sx={{
                                        fontWeight: 'bold',
                                        fontSize: '20px',
                                        fontFamily: 'Arial, sans-serif',
                                        letterSpacing: '1px',
                                        textTransform: 'uppercase',
                                        color: '#3b9298' }}
                                >
                                    Time left: {" "}
                                </Typography >
                                <Typography
                                    component="span"
                                    variant="subtitle1"
                                    sx={{
                                        fontWeight: 'bold',
                                        fontSize: '16px',
                                        fontFamily: 'Arial, sans-serif',
                                        color: '#333',
                                        letterSpacing: '1px' }}
                                >
                                 <CountdownTimer targetDate={endDateLocal} />
                                </Typography >
                            </Box>
                        </Grid>
                    </Box>
                </Box>

                <Box sx={{ padding: '20px', display: 'flex', flexDirection: 'column', alignItems: 'flex-start', textAlign: 'left'}}>
                    <Box>
                        <Typography component="span" variant="subtitle1" sx={{ fontWeight: 'bold', color: '#255e62', fontFamily: 'Arial, sans-serif', display: 'inline-block'}}>
                            Condition:&nbsp;
                        </Typography>
                        <Typography component="span" sx={{fontFamily: 'Arial, sans-serif', display: 'inline-block'}}>{auctionData.condition}</Typography>
                    </Box>
                    <Box sx={{ marginTop: '5px' }}>
                        <Typography component="span" variant="subtitle1" sx={{ fontWeight: 'bold', color: '#255e62', fontFamily: 'Arial, sans-serif', display: 'inline-block'}}>
                            Part category:&nbsp;
                        </Typography>
                        <Typography component="span" sx={{fontFamily: 'Arial, sans-serif', display: 'inline-block'}}>{categoryData.id}</Typography>
                    </Box>
                    {seriesData.name? (
                    <>
                    <Box>
                        <Typography component="span" variant="subtitle1" sx={{ fontWeight: 'bold', color: '#255e62', fontFamily: 'Arial, sans-serif', display: 'inline-block'}}>
                            Part series:&nbsp;
                        </Typography>
                        <Typography component="span" sx={{fontFamily: 'Arial, sans-serif', display: 'inline-block'}}>{seriesData.name}</Typography>
                    </Box>
                    </> ) : null}
                    <Box>
                        <Typography component="span" variant="subtitle1" sx={{ fontWeight: 'bold', color: '#255e62', fontFamily: 'Arial, sans-serif', display: 'inline-block'}}>
                            Part name:&nbsp;
                        </Typography>
                        <Typography component="span" sx={{fontFamily: 'Arial, sans-serif', display: 'inline-block'}}>{partData.name}</Typography>
                    </Box>
                    {auctionData.manufacturer ? (
                    <>
                    <Box sx={{ marginTop: '5px' }}>
                        <Typography component="span" variant="subtitle1" sx={{ fontWeight: 'bold', color: '#255e62', fontFamily: 'Arial, sans-serif', display: 'inline-block'}}>
                            Manufacturer:&nbsp;
                        </Typography>
                        <Typography component="span" sx={{fontFamily: 'Arial, sans-serif', display: 'inline-block'}}>{auctionData.manufacturer}</Typography>
                    </Box>
                    </> ) : null}
                    <Typography component="span" variant="subtitle1" sx={{ fontWeight: 'bold', color: '#255e62', fontFamily: 'Arial, sans-serif', marginTop: '5px'}}>
                        Description:&nbsp;
                    </Typography>
                    <Typography component="span" sx={{fontFamily: 'Arial, sans-serif', wordBreak: 'break-all' }}>{auctionData.description}</Typography>
                    <Typography component="span"     ariant="subtitle1" sx={{ fontWeight: 'bold', color: '#255e62', marginTop: '30px', fontFamily: 'Arial, sans-serif' }}>
                        Auction start date:&nbsp;
                    </Typography>
                    <Typography component="span" sx={{fontFamily: 'Arial, sans-serif' }}>{startDateLocal}</Typography>
                    <Typography component="span" variant="subtitle1" sx={{ fontWeight: 'bold', color: '#255e62', fontFamily: 'Arial, sans-serif', marginTop: '5px' }}>
                        Auction end date:&nbsp;
                    </Typography>
                    <Typography component="span" sx={{fontFamily: 'Arial, sans-serif' }}>{endDateLocal}</Typography>
                </Box>
                <AuctionRecommendations auctionId = {auctionId}/>
            </Box>
        </Container>
    );
};

export default AuctionInfo;
