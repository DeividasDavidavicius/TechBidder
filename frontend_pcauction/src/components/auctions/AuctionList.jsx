import { useEffect, useState } from "react";
import { useLocation, useNavigate } from "react-router-dom";
import PATHS from "../../utils/Paths";
import { Avatar, Box, Card, CardActionArea, CardContent, CardHeader, Container, CssBaseline, Pagination, Typography } from "@mui/material";
import { getAuctionsWithPagination } from "../../services/AuctionService";

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
            setAuctions(result.auctions);
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
          border: '1px solid #ccc',
          borderRadius: '10px',
          padding: '20px',
        }}
      >
        <Box>
          {auctions.map(auction => (
            <Card key = {auction.id}  display="flex">
               <CardActionArea onClick={() => handleCardClick(auction.id)} sx={{ width: '100%', marginBottom: 2, display: 'flex', '&:hover': { boxShadow: '0 0 10px rgba(0, 0, 0, 1)' } }}>
              <Box sx={{ flexGrow: 1 }}>
                <CardHeader
                  title={<Typography variant="h6">{auction.name}</Typography>}
                  sx={{ textAlign: 'left' }}
                />
                <CardContent>
                  <Typography
                    variant="body2"
                    color="textSecondary"
                    component="p"
                    sx={{
                      textAlign: 'left',
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
