import { useContext, useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { getAuction } from "../../services/AuctionService";
import SnackbarContext from "../../contexts/SnackbarContext";
import { Avatar, Box, Button, Container, CssBaseline, Grid, TextField, Typography } from "@mui/material";
import PATHS from "../../utils/Paths";
import { checkTokenValidity, refreshAccessToken } from "../../services/AuthService";
import { useUser } from "../../contexts/UserContext";
import { getHighestBid, postBid } from "../../services/BIdService";

function AuctionInfo() {
    const [auctionData, setAuctionData] = useState({});
    const { auctionId } = useParams();
    const [startDateLocal, setStartDateLocal] = useState(null);
    const [endDateLocal, setEndDateLocal] = useState(null);
    const [minIncrement, setMinIncrement] = useState(0);
    const [highestBid, setHighestBid] = useState(0);
    const [imageData, setImageData] = useState(null);
    const openSnackbar = useContext(SnackbarContext);
    const navigate = useNavigate();
    const { setLogin, setLogout } = useUser();
    const [bidAmountField, setBidAmountField] = useState(0);

    const handleBidChange = (event) => {
        setBidAmountField(event.target.value);
      };

    const handleSubmitBid = async (e) => {
        e.preventDefault();

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

        if (Object.keys(errors).length > 0) {
            openSnackbar(errors.bidAmount, 'error'); // TODO ADD TEXT BELOW
            return;
        }

        const data = {amount: bidAmount};
        await postBid(data, auctionId);

        openSnackbar('Bid placed!', 'success');
        setBidAmountField(0);
        setHighestBid(bidAmount);
        //window.location.reload();
        // navigate(PATHS.AUCTIONINFO.replace(":auctionId", auctionId));
    };

    useEffect(() => {
        const getAuctionData = async () => {
            try {
                const result = await getAuction(auctionId);
                setAuctionData(result);
                setImageData(result.imageData);

                const offsetInMilliseconds = new Date().getTimezoneOffset() * 60000;
                const utcDateStart = new Date(result.startDate);
                const utcDateEnd = new Date(result.endDate);

                const localDateStart = new Date(utcDateStart.getTime() - offsetInMilliseconds).toLocaleString();
                const localDateEnd = new Date(utcDateEnd.getTime() - offsetInMilliseconds).toLocaleString();

                setStartDateLocal(localDateStart);
                setEndDateLocal(localDateEnd);
                setMinIncrement(result.minIncrement);
            }
            catch {
                openSnackbar('This auction does not exist!', 'error');
                navigate(PATHS.MAIN);
            }
        };

        const getHighestBidData = async () => {
            try {
                const result = await getHighestBid(auctionId);
                setHighestBid(result.amount);
            }
            catch {
                openSnackbar('This auction does not exist!', 'error');
                navigate(PATHS.MAIN);
            }
        };

        getAuctionData();
        getHighestBidData();
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
                        src={`data:image/jpeg;base64,${imageData}`}
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
                    </Box>
                </Box>

                <Box sx={{ padding: '20px', display: 'flex', flexDirection: 'column', alignItems: 'flex-start', textAlign: 'left'}}>
                    <Typography component="span" variant="subtitle1" sx={{ fontWeight: 'bold', color: '#255e62', fontFamily: 'Arial, sans-serif'}}>
                        Condition:&nbsp;
                    </Typography>
                    <Typography component="span" sx={{fontFamily: 'Arial, sans-serif' }}>{auctionData.condition}</Typography>
                    {auctionData.manufacturer ? (
                    <>
                    <Typography component="span" variant="subtitle1" sx={{ fontWeight: 'bold', color: '#255e62', fontFamily: 'Arial, sans-serif', marginTop: '5px' }}>
                        Manufacturer:&nbsp;
                    </Typography>
                    <Typography component="span" sx={{fontFamily: 'Arial, sans-serif' }}>{auctionData.manufacturer}</Typography>
                    </> ) : null}
                    <Typography component="span" variant="subtitle1" sx={{ fontWeight: 'bold', color: '#255e62', fontFamily: 'Arial, sans-serif', marginTop: '5px'}}>
                        Description:&nbsp;
                    </Typography>
                    <Typography component="span" sx={{fontFamily: 'Arial, sans-serif' }}>{auctionData.description}</Typography>
                    <Typography component="span" variant="subtitle1" sx={{ fontWeight: 'bold', color: '#255e62', marginTop: '30px', fontFamily: 'Arial, sans-serif' }}>
                        Auction start date:&nbsp;
                    </Typography>
                    <Typography component="span" sx={{fontFamily: 'Arial, sans-serif' }}>{startDateLocal}</Typography>
                    <Typography component="span" variant="subtitle1" sx={{ fontWeight: 'bold', color: '#255e62', fontFamily: 'Arial, sans-serif', marginTop: '5px' }}>
                        Auction end date:&nbsp;
                    </Typography>
                    <Typography component="span" sx={{fontFamily: 'Arial, sans-serif' }}>{endDateLocal}</Typography>
                </Box>
            </Box>
        </Container>
    );
};

export default AuctionInfo;
