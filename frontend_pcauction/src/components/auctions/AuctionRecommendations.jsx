import { useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import PATHS from "../../utils/Paths";
import { Avatar, Box, Card, CardActionArea, CardContent, CardHeader, Container, CssBaseline, Pagination, Typography } from "@mui/material";
import { getAuction, getAuctionRecommendations } from "../../services/AuctionService";
import { timeLeft } from "../../utils/DateUtils";

function AuctionRecommendations() {
    const navigate = useNavigate();
    const { auctionId } = useParams();
    const [auctions, setAuctions] = useState([]);
    const [auctionData, setAuctionData] = useState({});

    useEffect(() => {
        const fetchAuctionsData = async () => {
            const result = await getAuctionRecommendations(auctionId);
            setAuctions(result);

            await fetchAuctionData();
        };

        const fetchAuctionData = async () => {
            const result = await getAuction(auctionId);
            setAuctionData(result);
        }

        fetchAuctionsData();
    }, []);

    const truncateText = (text, maxLength) => {
        if (text.length <= maxLength) return text;
        return text.slice(0, maxLength).trimEnd() + '...';
    };

    const handleCardClick = (auctionId) => {
        navigate(PATHS.AUCTIONINFO.replace(":auctionId", auctionId));
      };

    return (
    <Container component="main" maxWidth="lg">
      <CssBaseline />
      <Box
        sx={{
          marginTop: 8,
          border: '1px solid #ccc',
          borderRadius: '10px',
          padding: '20px',
        }}
      >
        <Box sx={{ marginTop: 2, marginBottom: 3, display: 'flex', flexDirection: 'column', alignItems: 'center' }}>
            <Typography component="h1" variant="h5" sx={{ fontSize: '26px', fontWeight: 'bold', fontFamily: 'Arial, sans-serif', color: '#0d6267' }}>
                Auction '{auctionData.name}' recommendations
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
    </Container>
    );
};

export default AuctionRecommendations;
