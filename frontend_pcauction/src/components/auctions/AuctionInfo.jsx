import { useContext, useEffect, useState } from "react";
import { useLocation, useNavigate, useParams } from "react-router-dom";
import { getAuction } from "../../services/AuctionService";
import SnackbarContext from "../../contexts/SnackbarContext";
import { Avatar, Box, Button, Container, CssBaseline, Dialog, DialogActions, DialogTitle, Grid, Paper, Table, TableBody, TableCell, TableHead, TableRow, TextField, Typography } from "@mui/material";
import PATHS from "../../utils/Paths";
import { checkTokenValidity, refreshAccessToken } from "../../services/AuthenticationService";
import { useUser } from "../../contexts/UserContext";
import { deleteBid, getAuctionBids, getHighestBid, postBid } from "../../services/BidService";
import { getCategory } from "../../services/PartCategoryService";
import { getPart } from "../../services/PartService";
import { getSeries } from "../../services/SeriesService";
import CountdownTimer from "./CountdownTimer";
import AuctionRecommendations from "./AuctionRecommendations";
import { loadStripe } from "@stripe/stripe-js";
import CancelOutlinedIcon from '@mui/icons-material/CancelOutlined';
import HighlightOffIcon from '@mui/icons-material/HighlightOff';
import UndoRoundedIcon from '@mui/icons-material/UndoRounded';
import { getPurchase, getStripePurchaseSession, patchPurchase } from "../../services/PurchaseService";
import { getUserData } from "../../services/UserService";

function AuctionInfo() {
    const [auctionData, setAuctionData] = useState({});
    const { auctionId } = useParams();
    const [startDateLocal, setStartDateLocal] = useState(null);
    const [endDateLocal, setEndDateLocal] = useState(null);
    const [currentDateLocal, setCurrentDateLocal] = useState(new Date().toLocaleString())
    const [minIncrement, setMinIncrement] = useState(0);
    const [highestBid, setHighestBid] = useState(0);
    const [imageUri, setImageUri] = useState(null);
    const openSnackbar = useContext(SnackbarContext);
    const navigate = useNavigate();
    const { role, setLogin, setLogout, getUserId } = useUser();
    const [bidAmountField, setBidAmountField] = useState(0);
    const [canBid, setCanBid] = useState(true);

    const [partData, setPartData] = useState([]);
    const [categoryData, setCategoryData] = useState([]);
    const [seriesData, setSeriesData] = useState([]);

    const location = useLocation();
    const queryParams = new URLSearchParams(location.search);
    const [purchase, setPurchase] = useState(null);
    const [purchaseStatus] = useState(queryParams.get('status'));

    const [bids, setBids] = useState([]);
    const [openRemoveModal, setOpenRemoveModal] = useState(false);
    const [removeBid, setRemoveBid] = useState({});

    const [buyerData, setBuyerData] = useState({});

    const handleBidChange = (event) => {
        setBidAmountField(event.target.value);
    };

    const handleOpenRemove = (bid) => {
        setRemoveBid(bid);
        setOpenRemoveModal(true);
    };

    const handleCloseRemove = () => {
        setOpenRemoveModal(false);
        setRemoveBid({});
    };

    const handleRemoveBid = async () => {
        const accessToken = localStorage.getItem('accessToken');
        if (!checkTokenValidity(accessToken)) {
            const result = await refreshAccessToken();
            if (!result.success) {
                openSnackbar('You must login to pay for auction!', 'error');
                setLogout();
                navigate(PATHS.LOGIN);
                return;
            }

            setLogin(result.response.data.accessToken, result.response.data.refreshToken);
        }

        await deleteBid(auctionId, removeBid.id)
        openSnackbar('Bid cancelled successfully!', 'success');

        const updatedBids = bids.filter(
            (bid) => bid.id !== removeBid.id
        );
        setBids(updatedBids);
        handleCloseRemove();
    }

    function isOlderBy30Minutes(date1, date2) {
        const d1 = new Date(date1);
        const d2 = new Date(date2);

        const timeDiff = d2 - d1;

        const minutesDiff = timeDiff / (1000 * 60);

        return minutesDiff >= 30;
    }

    const handlePayClick =  async () => {
        const accessToken = localStorage.getItem('accessToken');
        if (!checkTokenValidity(accessToken)) {
            const result = await refreshAccessToken();
            if (!result.success) {
                openSnackbar('You must login to pay for auction!', 'error');
                setLogout();
                navigate(PATHS.LOGIN);
                return;
            }

            setLogin(result.response.data.accessToken, result.response.data.refreshToken);
        }

        const stripe = await loadStripe("pk_test_51P4prORxCq26AiIxyvGnTqs397m678mlyguCvx5YNRQ309rpj0T41Jgqn4lnsWTqZtSCRoMKLhHK0OpvpxnwSytQ00IYTdA36o");

        const response = await getStripePurchaseSession(auctionId);

        try
        {
            await stripe.redirectToCheckout({sessionId: response.id});
        }
        catch(error) {}
    }

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

        try {
            const data = {amount: bidAmount};
            await postBid(data, auctionId);
        }
        catch(error) {}

        openSnackbar('Bid placed successfully!', 'success');
        setBidAmountField(0);
        setHighestBid(bidAmount);
        //window.location.reload(PATHS.AUCTIONINFO.replace(":auctionId", auctionId);
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
                await fetchPartData(result.categoryId, result.partId);
                if (!(role.includes("RegisteredUser")) || result.userId === getUserId()) {
                    setCanBid(false);
                }
            }
            catch(error) {
                openSnackbar('This auction does not exist!', 'error');
                navigate(PATHS.MAIN);
            }
        };

        const fetchHighestBidData = async () => {
            try {
                const result = await getHighestBid(auctionId);
                setHighestBid(result.amount);
            }
            catch(error) {
                openSnackbar('This auction does not exist!', 'error');
                navigate(PATHS.MAIN);
            }
        };

        const updatePurchaseStatus = async() => {
            if(purchaseStatus)
            {
                try {
                    await patchPurchase(auctionId);
                    window.location.replace(PATHS.AUCTIONINFO.replace(":auctionId", auctionId));
                }
                catch(error) {}
            }
        };

        fetchAuctionData();
        fetchHighestBidData();
        updatePurchaseStatus();

      }, [auctionId]);


      useEffect(() => {
        const fetchHighestBidData = async () => {
            try {
                const result = await getHighestBid(auctionId);
                setHighestBid(result.amount);
            }
            catch(error) {
                openSnackbar('This auction does not exist!', 'error');
                navigate(PATHS.MAIN);
            }
            setCurrentDateLocal(new Date().toLocaleString());
        };

        fetchHighestBidData();

        const interval = setInterval(fetchHighestBidData, 3000);

        return () => clearInterval(interval);
    }, [auctionId]);

    useEffect(() => {
        const fetchPurchase = async () => {
            try {
                const purchaseResult = await getPurchase(auctionId);
                setPurchase(purchaseResult);

                const buyerResult = await getUserData(purchaseResult.buyerId);
                setBuyerData(buyerResult);

                console.log(buyerResult);
            } catch (error) {}
        }

        fetchPurchase();

        const interval = setInterval(fetchPurchase, 3000); //

        return () => clearInterval(interval);
    }, [auctionId]);

    function modifyDate(dateString) {
        const offsetInMilliseconds = new Date().getTimezoneOffset() * 60000;
        const utcDate = new Date(dateString);
        const localDate = new Date(utcDate.getTime() - offsetInMilliseconds).toLocaleString();
        return localDate;
      }

    useEffect(() => {
        const fetchBids = async() => {
            const result = await getAuctionBids(auctionId);

            const modifiedBids = result.map(bid => {
                return {
                  ...bid,
                  creationDate: modifyDate(bid.creationDate)
                };
              });

            setBids(modifiedBids);
        }

        fetchBids();

        const interval = setInterval(fetchBids, 3000);

        return () => clearInterval(interval);
    }, [auctionId]);

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
                                &nbsp;{minIncrement}€
                                </Typography >
                            </Box>
                        </Grid>

                        {partData.averagePrice > 0 &&
                        <Grid item xs={12}>
                            <Box sx={{ display: 'flex', alignItems: 'stretch' }}>
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
                                    Average price:
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
                                &nbsp;{partData.averagePrice}€
                                </Typography >
                            </Box>
                        </Grid>
                        }

                        {canBid === true && endDateLocal > currentDateLocal && startDateLocal < currentDateLocal && (
                        <>
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
                        </> )
                        }
                        { !(role.includes("RegisteredUser")) && endDateLocal > currentDateLocal && startDateLocal < currentDateLocal &&
                            <Grid item xs={12}>
                                <Box sx={{ display: 'flex', alignItems: 'stretch' }}>
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
                                        Login to bid!
                                    </Typography >
                                </Box>
                            </Grid>
                        }


                        <Grid item xs={12}>
                            {startDateLocal < currentDateLocal && endDateLocal > currentDateLocal && (
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
                            )}
                            {startDateLocal > currentDateLocal && (
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
                                Auction starts in: {" "}
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
                                <CountdownTimer targetDate={startDateLocal} />
                            </Typography >
                            </Box>
                            )}
                            {endDateLocal < currentDateLocal && auctionData.status !== 'Cancelled' && (
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
                                    Auction has ended
                                </Typography >
                            </Box>
                            )}
                            {auctionData.status === 'Cancelled' && (
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
                                    Auction was cancelled
                                </Typography >
                            </Box>
                            )}
                        </Grid>

                        {endDateLocal < currentDateLocal && purchase && role.includes("RegisteredUser") && purchase.buyerId === getUserId() && purchase.status === 'PendingPayment' &&
                        <Grid item xs={12}>
                            <Box sx={{ display: 'flex', alignItems: 'stretch' }}>
                                <Button
                                    variant="contained"
                                    color="primary"
                                    sx={{
                                        width: '25%',
                                        borderTopLeftRadius: '10px',
                                        borderBottomLeftRadius: '10px',
                                        borderTopRightRadius: '10px',
                                        borderBottomRightRadius: '10px',
                                        bgcolor: '#0d6267',
                                        fontSize: '14px',
                                        fontWeight: 'bold',
                                        '&:hover': {
                                            backgroundColor: '#3d8185',
                                        },
                                        marginTop: 2
                                    }}
                                    onClick={handlePayClick}
                                >
                                    PAY
                                </Button>
                            </Box>
                        </Grid>
                        }

                        {purchase && purchase.status !== 'PendingPayment' &&
                        <Grid item xs={12}>
                            <Box sx={{ display: 'flex', alignItems: 'stretch' }}>
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
                                    {purchase.status === 'Paid' ? 'Buyer paid for the part' : 'Buyer did not pay for the part in time'}
                                    </Typography >
                            </Box>
                            {purchase.status === 'Paid' && buyerData.address !== null && buyerData.address !== "" && role.includes("RegisteredUser") && auctionData.userId === getUserId() &&
                                <Box sx={{ display: 'flex', alignItems: 'stretch', mt: 2 }}>
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
                                        Address:
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
                                    &nbsp;{buyerData.address}
                                    </Typography >
                                </Box>
                            }
                            {purchase.status === 'Paid' && buyerData.phoneNumber !== null && buyerData.phoneNumber !== "" && role.includes("RegisteredUser") && auctionData.userId === getUserId() &&
                                <Box sx={{ display: 'flex', alignItems: 'stretch' }}>
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
                                        Phone number:
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
                                    &nbsp;{buyerData.phoneNumber}
                                    </Typography >
                                </Box>
                            }
                            {purchase.status === 'Paid' && buyerData.bankDetails !== null && buyerData.bankDetails !== "" && role.includes("RegisteredUser") && auctionData.userId === getUserId() &&
                                <Box sx={{ display: 'flex', alignItems: 'stretch' }}>
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
                                        Bank details:
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
                                    &nbsp;{buyerData.bankDetails}
                                    </Typography >
                                </Box>
                            }
                        </Grid>
                        }

                        {endDateLocal < currentDateLocal && purchase && purchase.status === 'PendingPayment' && role.includes("RegisteredUser") && purchase.buyerId !== getUserId() &&
                        <Grid item xs={12}>
                            <Box sx={{ display: 'flex', alignItems: 'stretch' }}>
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
                                    Waiting for buyer to pay for the part
                                    </Typography >
                            </Box>
                        </Grid>
                        }

                    </Box>
                </Box>

                <Box sx={{ padding: '20px', display: 'flex', flexDirection: 'column', alignItems: 'flex-start', textAlign: 'left'}}>
                    <Box sx={{ display: 'flex', flexDirection: 'row', justifyContent: 'space-between', width: '100%' }}>
                        <Box sx={{ width: '50%' }}>
                            <Box>
                                <Typography component="span" variant="subtitle1" sx={{ fontWeight: 'bold', color: '#1c9fa9', fontFamily: 'Arial, sans-serif', display: 'inline-block'}}>
                                    Part information&nbsp;
                                </Typography>
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
                            <Box sx={{marginTop: '5px'}}>
                                <Typography component="span" variant="subtitle1" sx={{ fontWeight: 'bold', color: '#255e62', fontFamily: 'Arial, sans-serif', display: 'inline-block'}}>
                                    Manufacturer:&nbsp;
                                </Typography>
                                <Typography component="span" sx={{fontFamily: 'Arial, sans-serif', display: 'inline-block'}}>{auctionData.manufacturer}</Typography>
                            </Box>
                            </> ) : null}
                            <Box>
                                <Typography component="span" variant="subtitle1" sx={{ fontWeight: 'bold', color: '#255e62', fontFamily: 'Arial, sans-serif', display: 'inline-block'}}>
                                    Condition:&nbsp;
                                </Typography>
                                <Typography component="span" sx={{fontFamily: 'Arial, sans-serif', display: 'inline-block'}}>{auctionData.condition}</Typography>
                            </Box>
                        </Box>

                        {partData.type !== "Temporary" &&
                            <Box sx={{ width: '50%' }}>
                                <Box>
                                    <Typography component="span" variant="subtitle1" sx={{ fontWeight: 'bold', color: '#1c9fa9', fontFamily: 'Arial, sans-serif', display: 'inline-block'}}>
                                        Specifications&nbsp;
                                    </Typography>
                                </Box>
                                {partData.specificationValue1 !== '' && (
                                <>
                                <Box>
                                    <Typography component="span" variant="subtitle1" sx={{ fontWeight: 'bold', color: '#255e62', fontFamily: 'Arial, sans-serif', display: 'inline-block'}}>
                                        {categoryData.specificationName1}:&nbsp;
                                    </Typography>
                                    <Typography component="span" sx={{fontFamily: 'Arial, sans-serif', display: 'inline-block'}}>{partData.specificationValue1}</Typography>
                                </Box>
                                </> )}
                                {partData.specificationValue2 !== '' && (
                                <>
                                <Box>
                                    <Typography component="span" variant="subtitle1" sx={{ fontWeight: 'bold', color: '#255e62', fontFamily: 'Arial, sans-serif', display: 'inline-block'}}>
                                        {categoryData.specificationName2}:&nbsp;
                                    </Typography>
                                    <Typography component="span" sx={{fontFamily: 'Arial, sans-serif', display: 'inline-block'}}>{partData.specificationValue2}</Typography>
                                </Box>
                                </> )}
                                {partData.specificationValue3 !== '' && (
                                <>
                                <Box>
                                    <Typography component="span" variant="subtitle1" sx={{ fontWeight: 'bold', color: '#255e62', fontFamily: 'Arial, sans-serif', display: 'inline-block'}}>
                                        {categoryData.specificationName3}:&nbsp;
                                    </Typography>
                                    <Typography component="span" sx={{fontFamily: 'Arial, sans-serif', display: 'inline-block'}}>{partData.specificationValue3}</Typography>
                                </Box>
                                </> )}
                                {partData.specificationValue4 !== '' && (
                                <>
                                <Box>
                                    <Typography component="span" variant="subtitle1" sx={{ fontWeight: 'bold', color: '#255e62', fontFamily: 'Arial, sans-serif', display: 'inline-block'}}>
                                        {categoryData.specificationName4}:&nbsp;
                                    </Typography>
                                    <Typography component="span" sx={{fontFamily: 'Arial, sans-serif', display: 'inline-block'}}>{partData.specificationValue4}</Typography>
                                </Box>
                                </> )}
                                {partData.specificationValue5 !== '' && (
                                <>
                                <Box>
                                    <Typography component="span" variant="subtitle1" sx={{ fontWeight: 'bold', color: '#255e62', fontFamily: 'Arial, sans-serif', display: 'inline-block'}}>
                                        {categoryData.specificationName5}:&nbsp;
                                    </Typography>
                                    <Typography component="span" sx={{fontFamily: 'Arial, sans-serif', display: 'inline-block'}}>{partData.specificationValue5}</Typography>
                                </Box>
                                </> )}
                                {partData.specificationValue6 !== '' && (
                                <>
                                <Box>
                                    <Typography component="span" variant="subtitle1" sx={{ fontWeight: 'bold', color: '#255e62', fontFamily: 'Arial, sans-serif', display: 'inline-block'}}>
                                        {categoryData.specificationName6}:&nbsp;
                                    </Typography>
                                    <Typography component="span" sx={{fontFamily: 'Arial, sans-serif', display: 'inline-block'}}>{partData.specificationValue6}</Typography>
                                </Box>
                                </> )}
                                {partData.specificationValue7 !== '' && (
                                <>
                                <Box>
                                    <Typography component="span" variant="subtitle1" sx={{ fontWeight: 'bold', color: '#255e62', fontFamily: 'Arial, sans-serif', display: 'inline-block'}}>
                                        {categoryData.specificationName7}:&nbsp;
                                    </Typography>
                                    <Typography component="span" sx={{fontFamily: 'Arial, sans-serif', display: 'inline-block'}}>{partData.specificationValue7}</Typography>
                                </Box>
                                </> )}
                                {partData.specificationValue8 !== '' && (
                                <>
                                <Box>
                                    <Typography component="span" variant="subtitle1" sx={{ fontWeight: 'bold', color: '#255e62', fontFamily: 'Arial, sans-serif', display: 'inline-block'}}>
                                        {categoryData.specificationName8}:&nbsp;
                                    </Typography>
                                    <Typography component="span" sx={{fontFamily: 'Arial, sans-serif', display: 'inline-block'}}>{partData.specificationValue8}</Typography>
                                </Box>
                                </> )}
                                {partData.specificationValue9 !== '' && (
                                <>
                                <Box>
                                    <Typography component="span" variant="subtitle1" sx={{ fontWeight: 'bold', color: '#255e62', fontFamily: 'Arial, sans-serif', display: 'inline-block'}}>
                                        {categoryData.specificationName9}:&nbsp;
                                    </Typography>
                                    <Typography component="span" sx={{fontFamily: 'Arial, sans-serif', display: 'inline-block'}}>{partData.specificationValue9}</Typography>
                                </Box>
                                </> )}
                                {partData.specificationValue10 !== '' && (
                                <>
                                <Box>
                                    <Typography component="span" variant="subtitle1" sx={{ fontWeight: 'bold', color: '#255e62', fontFamily: 'Arial, sans-serif', display: 'inline-block'}}>
                                        {categoryData.specificationName10}:&nbsp;
                                    </Typography>
                                    <Typography component="span" sx={{fontFamily: 'Arial, sans-serif', display: 'inline-block'}}>{partData.specificationValue10}</Typography>
                                </Box>
                                </> )}
                            </Box>
                        }
                    </Box>
                    <Typography component="span" variant="subtitle1" sx={{ fontWeight: 'bold', color: '#255e62', fontFamily: 'Arial, sans-serif', marginTop: 3}}>
                            Description:&nbsp;
                    </Typography>
                    <Typography component="span" sx={{fontFamily: 'Arial, sans-serif', wordBreak: 'break-all' }}>{auctionData.description}</Typography>
                </Box>
                {bids && bids.length > 0 &&
                <Box sx={{ marginTop: 3, marginBottom: 2, display: 'flex', flexDirection: 'column', alignItems: 'center' }}>
                    <Typography component="h1" variant="h5" sx={{ fontSize: '26px', fontWeight: 'bold', fontFamily: 'Arial, sans-serif', color: '#0d6267' }}>
                        BIDS
                    </Typography>
                </Box>
                }
                {bids && bids.length > 0 &&
                    <Box sx={{marginRight: 5, marginLeft: 5}}>
                        <Paper style={{ maxHeight: '300px', overflow: 'auto' }}>
                            <Table size="small">
                                <TableHead>
                                    <TableRow>
                                        <TableCell style={{ backgroundColor: '#0d6267', color: 'white', fontWeight: 'bold' }}>USERNAME</TableCell>
                                        <TableCell style={{ backgroundColor: '#0d6267',  color: 'white', fontWeight: 'bold' }}>DATE</TableCell>
                                        <TableCell style={{ backgroundColor: '#0d6267', color: 'white', fontWeight: 'bold' }}>AMOUNT</TableCell>
                                        <TableCell style={{ backgroundColor: '#0d6267', color: 'white', fontWeight: 'bold' }}></TableCell>

                                    </TableRow>
                                </TableHead>
                                <TableBody>
                                {bids.map((b, index) => (
                                    <TableRow key={index}>
                                        <TableCell>{b.username}</TableCell>
                                        <TableCell>{b.creationDate}</TableCell>
                                        <TableCell>{b.amount}€</TableCell>
                                        <TableCell>
                                            {b.creationDate.getTime}
                                            {index === 0 && role.includes("RegisteredUser") && b.userId === getUserId() && isOlderBy30Minutes(currentDateLocal, endDateLocal) === true &&
                                            <Button startIcon={<CancelOutlinedIcon />}
                                                sx={{ marginRight: 0, color: '#138c94', fontWeight: 'bold' }}
                                                onClick={ () => handleOpenRemove(b)}
                                            >
                                                Cancel
                                            </Button>
                                            }
                                        </TableCell>
                                    </TableRow>
                                ))}
                                </TableBody>
                            </Table>
                        </Paper>
                    </Box>
                }
                <AuctionRecommendations auctionId = {auctionId}/>
            </Box>
            <Dialog open={openRemoveModal} onClose={handleCloseRemove}>
                <DialogTitle sx={{ fontSize: '20px', fontWeight: 'bold', fontFamily: 'Arial, sans-serif', color: '#0d6267' }} >Do you want to cancel this bid ({removeBid.amount}€)?</DialogTitle>
                <DialogActions style={{ justifyContent: 'center' }}>
                    <Button onClick={handleRemoveBid} startIcon={<HighlightOffIcon />} sx ={{ fontWeight: 'bold', color: "red" }}>
                        Confirm
                    </Button>
                    <Button onClick={handleCloseRemove} startIcon={<UndoRoundedIcon />} sx ={{ fontWeight: 'bold', color: "#268747" }}>
                        Back
                    </Button>
                </DialogActions>
            </Dialog>
        </Container>
    );
};

export default AuctionInfo;
