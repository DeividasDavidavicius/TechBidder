import { useContext, useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import PATHS from "../../utils/Paths";
import { Avatar, Box, Card, CardActionArea, CardContent, CardHeader, Container, CssBaseline, Typography } from "@mui/material";
import { getAuctionRecommendations } from "../../services/AuctionService";
import { timeLeft } from "../../utils/DateUtils";
import SnackbarContext from "../../contexts/SnackbarContext";
import { getHighestBid } from "../../services/BIdService";
import { Link } from "react-router-dom";

const AuctionRecommendations = ({auctionId}) => {
    const navigate = useNavigate();
    const [auctions, setAuctions] = useState([]);
    const openSnackbar = useContext(SnackbarContext);

    useEffect(() => {
        const fetchAuctionsData = async () => {
          try {
            const result = await getAuctionRecommendations(auctionId);

            const auctionsWithHighestBid = await Promise.all(
              result.map(async (auction) => {
                const highestBid = await getHighestBid(auction.id);
                const highestBidAmount = highestBid.amount;
                return { ...auction, highestBidAmount };
              })
            );

            setAuctions(auctionsWithHighestBid);
          }
          catch(error)
          {
            openSnackbar('This auction does not exist!', 'error');
            navigate(PATHS.MAIN);
          }
        };

        window.scrollTo({ top: 0});
        fetchAuctionsData();
    }, [auctionId, navigate, openSnackbar]);

    const truncateText = (text, maxLength) => {
        if (text.length <= maxLength) return text;
        return text.slice(0, maxLength).trimEnd() + '...';
    };

    return (
    <Container component="main" maxWidth="lg">
      <CssBaseline />
      {auctions.length ? (
      <Box
        sx={{
          marginTop: 7,
          padding: '20px',
        }}
      >
        <Box sx={{ marginTop: 2, marginBottom: 3, display: 'flex', flexDirection: 'column', alignItems: 'center' }}>
          <Typography component="h1" variant="h5" sx={{ fontSize: '26px', fontWeight: 'bold', fontFamily: 'Arial, sans-serif', color: '#0d6267' }}>
            SIMILAR AUCTIONS
          </Typography>
        </Box>
        <Box>
          {auctions.map((auction, index) => (
            <Card
              key = {auction.id}
              display="flex"
              sx={{ marginBottom: 2, border: '1px solid #ddd' }}
            >
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
    );
};

export default AuctionRecommendations;
