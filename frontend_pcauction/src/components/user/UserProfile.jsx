import { AppBar, Avatar, Box, Button, Card, CardActionArea, CardContent, CardHeader, Container, CssBaseline, Paper, Tab, Table, TableBody, TableCell, TableHead, TableRow, Tabs, Typography } from "@mui/material";
import { useUser } from "../../contexts/UserContext";
import { useContext, useEffect, useState } from "react";
import PATHS from "../../utils/Paths";
import ModeEditIcon from '@mui/icons-material/ModeEdit';
import SnackbarContext from "../../contexts/SnackbarContext";
import { Link, useNavigate } from "react-router-dom";
import { getHighestBid, getUserBids, getWinningUserBids } from "../../services/BidService";
import { checkTokenValidity, refreshAccessToken } from "../../services/AuthenticationService";
import { getUserActiveAuctions, getUserEndedAuctions, getUserNewAuctions, getUserWonAuctions } from "../../services/AuctionService";
import { timeLeft } from "../../utils/DateUtils";

function UserProfile() {
    const { role, setLogin, setLogout, getUserId, getUserName } = useUser();
    const openSnackbar = useContext(SnackbarContext);
    const navigate = useNavigate();
    const [tabValue, setTabValue] = useState(0);

    const [allBids, setAllBids] = useState([]);
    const [winningBids, setWinningBids] = useState([]);

    const [newAuctions, setNewAuctions] = useState([]);
    const [activeAuctions, setActiveAuctions] = useState([]);
    const [endedAuctions, setEndedAuctions] = useState([]);
    const [wonAuctions, setWonAuctions] = useState([]);

    const handleTabChange = (event, newValue) => {
        setTabValue(newValue);
    };

    const handleRowClick = (auctionId) => {
        navigate(PATHS.AUCTIONINFO.replace(":auctionId", auctionId));
    };

    const truncateText = (text, maxLength) => {
        if (text.length <= maxLength) return text;
        return text.slice(0, maxLength).trimEnd() + '...';
    };

    function modifyDate(dateString) {
        const offsetInMilliseconds = new Date().getTimezoneOffset() * 60000;
        const utcDate = new Date(dateString);
        const localDate = new Date(utcDate.getTime() - offsetInMilliseconds).toLocaleString();
        return localDate;
    };

    useEffect(() => {
        const fetchData = async () => {
            if (!role.includes("RegisteredUser")) {
                openSnackbar('You must login to access your profile!', 'error');
                navigate(PATHS.LOGIN);
            }

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

            const allBidsResult = await getUserBids();

            const allBidsModified = allBidsResult.map(bid => {
                return {
                  ...bid,
                  creationDate: modifyDate(bid.creationDate)
                };
            });

            setAllBids(allBidsModified);


            const winningBidsResult = await getWinningUserBids();

            const winningBidsModified = winningBidsResult.map(bid => {
                return {
                  ...bid,
                  creationDate: modifyDate(bid.creationDate)
                };
            });

            setWinningBids(winningBidsModified);

            const newAuctionsResult = await getUserNewAuctions();
            setNewAuctions(newAuctionsResult);

            const activeAuctionsResult = await getUserActiveAuctions();

            await Promise.all(activeAuctionsResult.map(async (auction) => {
                const highestBidResult = await getHighestBid(auction.id);
                auction.highestBid = highestBidResult.amount;
            }));

            setActiveAuctions(activeAuctionsResult);

            const endedAuctionsResult = await getUserEndedAuctions();
            setEndedAuctions(endedAuctionsResult);

            const wonAuctionsResult = await getUserWonAuctions();
            setWonAuctions(wonAuctionsResult);
        }

        fetchData();
    }, []);


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
                { role.includes("RegisteredUser") &&
                    <Box sx={{ marginTop: 2, display: 'flex', flexDirection: 'column', alignItems: 'center' }}>
                        <Typography component="h1" variant="h5" sx={{ fontSize: '26px', fontWeight: 'bold', fontFamily: 'Arial, sans-serif', color: '#0d6267' }}>
                            USER '{getUserName().toUpperCase()}' PROFILE
                        </Typography>
                    </Box>
                }
                <Box sx={{
                    display: 'flex',
                    flexDirection: 'column',
                    justifyContent: 'center',
                    alignItems: 'center',
                    marginTop: 2
                }}>
                    <Box sx={{ borderBottom: 1, borderColor: 'divider', width: '100%', maxWidth: '90%' }}>
                        <AppBar position="static" sx={{ backgroundColor: '#fff', color: '#000' }}>
                            <Tabs value={tabValue} onChange={handleTabChange} aria-label="basic tabs example">
                                <Tab label="My bids" />
                                <Tab label="My auctions" />
                                <Tab label="Won auctions" />
                            </Tabs>
                        </AppBar>
                        {tabValue === 0 && (
                            <Box sx={{mt: 3}}>
                                { allBids && allBids.length > 0 &&
                                    <Box>
                                        <Typography variant="h5" sx={{fontWeight: 'bold', mt: 5}}>WINNING BIDS</Typography>
                                        <Box>
                                            <Paper style={{ maxHeight: '300px', overflow: 'auto' }}>
                                                <Table size="small">
                                                    <TableHead>
                                                        <TableRow>
                                                            <TableCell style={{ backgroundColor: '#0d6267',  color: 'white', fontWeight: 'bold' }}>DATE</TableCell>
                                                            <TableCell style={{ backgroundColor: '#0d6267', color: 'white', fontWeight: 'bold' }}>AMOUNT</TableCell>
                                                            <TableCell style={{ backgroundColor: '#0d6267', color: 'white', fontWeight: 'bold' }}>AUCTION</TableCell>
                                                        </TableRow>
                                                    </TableHead>
                                                    <TableBody>
                                                    {winningBids.map((b, index) => (
                                                        <TableRow key={index} onClick={() => handleRowClick(b.auctionId)}>
                                                            <TableCell>{b.creationDate}</TableCell>
                                                            <TableCell>{b.amount}€</TableCell>
                                                            <TableCell>{b.auctionName}</TableCell>
                                                        </TableRow>
                                                    ))}
                                                    </TableBody>
                                                </Table>
                                            </Paper>
                                        </Box>
                                    </Box>
                                }


                                { allBids && allBids.length > 0 &&
                                    <Box sx={{mt:3, mb: 3}}>
                                        <Typography variant="h5" sx={{fontWeight: 'bold'}}>ALL BIDS</Typography>
                                        <Box>
                                            <Paper style={{ maxHeight: '300px', overflow: 'auto' }}>
                                                <Table size="small">
                                                    <TableHead>
                                                        <TableRow>
                                                            <TableCell style={{ backgroundColor: '#0d6267',  color: 'white', fontWeight: 'bold' }}>DATE</TableCell>
                                                            <TableCell style={{ backgroundColor: '#0d6267', color: 'white', fontWeight: 'bold' }}>AMOUNT</TableCell>
                                                            <TableCell style={{ backgroundColor: '#0d6267', color: 'white', fontWeight: 'bold' }}>AUCTION</TableCell>
                                                        </TableRow>
                                                    </TableHead>
                                                    <TableBody>
                                                    {allBids.map((b, index) => (
                                                        <TableRow key={index} onClick={() => handleRowClick(b.auctionId)}>
                                                            <TableCell>{b.creationDate}</TableCell>
                                                            <TableCell>{b.amount}€</TableCell>
                                                            <TableCell>{b.auctionName}</TableCell>
                                                        </TableRow>
                                                    ))}
                                                    </TableBody>
                                                </Table>
                                            </Paper>
                                        </Box>
                                    </Box>
                                }
                            </Box>
                        )}
                        {tabValue === 1 && (
                            <Box sx={{mt: 3}}>
                                { newAuctions && newAuctions.length > 0 &&
                                    <Box sx={{mb: 3}}>
                                        <Typography variant="h5" sx={{fontWeight: 'bold'}}>UPCOMING AUCTIONS</Typography>
                                        <Box sx={{ overflowY: 'auto', maxHeight: '420px' }}>
                                            {newAuctions.map((auction, index) => (
                                                <Card key = {auction.id}  display="flex" sx={{ marginBottom: 1, border: '1px solid #ddd' }}>
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
                                                                        Auction starts in:&nbsp;
                                                                    </Typography>
                                                                    <Typography component="span" sx={{fontFamily: 'Arial, sans-serif', fontWeight: 'bold', color: '#c21818', display: 'inline-block'}}>
                                                                    {timeLeft(new Date().toISOString().slice(0, 19), auction.startDate)}
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
                                                                <Box sx ={{textAlign: 'left', mt: 1}}>
                                                                    <Link to={PATHS.EDITAUCTION.replace(':auctionId', auction.id).replace(':categoryId', auction.id.categoryId)}>
                                                                        <Button startIcon={<ModeEditIcon />} sx={{ marginRight: 3, color: '#138c94', fontWeight: 'bold' }}>
                                                                            Edit auction info
                                                                        </Button>
                                                                    </Link>
                                                                </Box>
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
                                }

                                { activeAuctions && activeAuctions.length > 0 &&
                                    <Box sx={{mb: 3}}>
                                        <Typography variant="h5" sx={{fontWeight: 'bold'}}>ACTIVE AUCTIONS</Typography>
                                        <Box sx={{ overflowY: 'auto', maxHeight: '420px' }}>
                                            {activeAuctions.map((auction, index) => (
                                                <Card key = {auction.id}  display="flex" sx={{ marginBottom: 1, border: '1px solid #ddd' }}>
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
                                                                { (auction.status === "New" || auction.status === "NewNA" ||  (auction.status === "Active" || auction.status === "ActiveNA") && auction.highestBid <= 0) &&
                                                                <Box sx ={{textAlign: 'left'}}>
                                                                    <Link to={PATHS.EDITAUCTION.replace(':auctionId', auction.id).replace(':categoryId', auction.id.categoryId)}>
                                                                        <Button startIcon={<ModeEditIcon />} sx={{ marginRight: 3, color: '#138c94', fontWeight: 'bold' }}>
                                                                            Edit auction info
                                                                        </Button>
                                                                    </Link>
                                                                </Box>
                                                                }
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
                                }

                                { endedAuctions && endedAuctions.length > 0 &&
                                    <Box sx={{mb: 3}}>
                                        <Typography variant="h5" sx={{fontWeight: 'bold'}}>ENDED AUCTIONS</Typography>
                                        <Box sx={{ overflowY: 'auto', maxHeight: '420px' }}>
                                            {endedAuctions.map((auction, index) => (
                                                <Card key = {auction.id}  display="flex" sx={{ marginBottom: 1, border: '1px solid #ddd' }}>
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
                                                                <Box sx ={{textAlign: 'left', mt: 1, marginRight: 3, color: '#138c94', fontWeight: 'bold', fontSize: '18px' }}>
                                                                    {auction.status == 'EndedWithBids' ? 'Awaiting payment' : auction.status == 'EndedWithoutBids' ? 'Ended without bids' : 'Paid'}
                                                                </Box>
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
                                }
                            </Box>
                        )}
                        {tabValue === 2 && (
                            <Box sx={{mt: 3}}>
                            { wonAuctions && wonAuctions.length > 0 &&
                                <Box sx={{mb: 3}}>
                                    <Typography variant="h5" sx={{fontWeight: 'bold'}}>WON AUCTIONS</Typography>
                                    <Box sx={{ overflowY: 'auto', maxHeight: '420px' }}>
                                        {wonAuctions.map((auction, index) => (
                                            <Card key = {auction.id}  display="flex" sx={{ marginBottom: 1, border: '1px solid #ddd' }}>
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
                                                            <Box sx ={{textAlign: 'left', mt: 1, marginRight: 3, color: '#138c94', fontWeight: 'bold', fontSize: '18px' }}>
                                                                {auction.status == 'EndedWithBids' ? 'Awaiting payment' : 'Paid'}
                                                            </Box>
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
                            }
                        </Box>
                        )}
                    </Box>
                </Box>
            </Box>
        </Container>
    );
}

export default UserProfile;
