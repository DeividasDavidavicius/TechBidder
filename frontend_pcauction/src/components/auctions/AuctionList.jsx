import { useEffect, useState } from "react";
import { useLocation, useNavigate } from "react-router-dom";
import PATHS from "../../utils/Paths";
import { Avatar, Box, Card, CardActionArea, CardContent, CardHeader, Container, CssBaseline, Pagination, Typography } from "@mui/material";
import { getAuctionsWithPagination } from "../../services/AuctionService";
import { timeLeft } from "../../utils/DateUtils";
import { getHighestBid } from "../../services/BIdService";

function AuctionList() {
    const navigate = useNavigate();

    const location = useLocation();
    const queryParams = new URLSearchParams(location.search);

    const [currentPage, setCurrentPage] = useState(parseInt(queryParams.get('page', 10) || 1));
    const [totalAuctions, setTotalAuctions] = useState(0);
    const [auctions, setAuctions] = useState([]);
    const auctionsPerPage = 5;

    useEffect(() => {
        const fetchAuctionsData = async () => {
            const result = await getAuctionsWithPagination(currentPage);
            setTotalAuctions(result.auctionCount);

            const auctionsWithHighestBid = await Promise.all(
              (result.auctions).map(async (auction) => {
                const highestBid = await getHighestBid(auction.id);
                const highestBidAmount = highestBid.amount;
                return { ...auction, highestBidAmount };
              })
            );

            setAuctions(auctionsWithHighestBid);
            window.scrollTo({ top: 0, behavior: 'smooth' });
        };

        fetchAuctionsData();
    }, [currentPage]);

    const handlePageChange = (event, newPage) => {
        navigate(PATHS.AUCTIONS + "?page=" + newPage);
        setCurrentPage(newPage);
    };

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
          //border: '1px solid #ccc',
          //borderRadius: '10px',
          padding: '20px',
        }}
      >
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
            </Card>
          ))}
        </Box>
        <Pagination
          count={Math.ceil(totalAuctions / auctionsPerPage)}
          page={currentPage}
          onChange={handlePageChange}
          sx={{ marginTop: '20px', display: 'flex', justifyContent: 'center' }}
        />
      </Box>
    </Container>
    );
};

export default AuctionList;
