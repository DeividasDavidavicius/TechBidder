import { useContext, useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { getAuction } from "../../services/AuctionService";
import SnackbarContext from "../../contexts/SnackbarContext";
import { Box, Container, CssBaseline, Typography } from "@mui/material";

function AuctionInfo() {
    const [auctionData, setAuctionData] = useState({});
    const { auctionId } = useParams();
    const [startDateLocal, setStartDateLocal] = useState(null);
    const [endDateLocal, setEndDateLocal] = useState(null);
    const openSnackbar = useContext(SnackbarContext);
    const navigate = useNavigate();

    useEffect(() => {
        const getAuctionData = async () => {
            const result = await getAuction(auctionId);
            setAuctionData(result);

            console.log(result);

            const offsetInMilliseconds = new Date().getTimezoneOffset() * 60000;
            const utcDateStart = new Date(result.startDate);
            const utcDateEnd = new Date(result.endDate);

            const localDateStart = new Date(utcDateStart.getTime() - offsetInMilliseconds).toLocaleString();
            const localDateEnd = new Date(utcDateEnd.getTime() - offsetInMilliseconds).toLocaleString();

            setStartDateLocal(localDateStart);
            setEndDateLocal(localDateEnd);
        };

        getAuctionData();
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
                <Box
                sx={{
                marginTop: 2,
                display: 'flex',
                flexDirection: 'column',
                alignItems: 'center',
                }}
                >
                    <Typography component="h1" variant="h5">
                    Auction '{auctionData.name}'
                    </Typography>
                </Box>
                <Box
                sx={{
                padding: '20px',
                display: 'flex',
                flexDirection: 'column',
                alignItems: 'flex-start',
                }}
                >
                    <Typography component="span" variant="subtitle1" sx={{ fontWeight: 'bold' }}>
                        Condition:&nbsp;
                    </Typography>
                    <Typography component="span">
                        {auctionData.condition}
                    </Typography>

                    <Typography component="span" variant="subtitle1" sx={{ fontWeight: 'bold' }}>
                        Manufacturer:&nbsp;
                    </Typography>
                    <Typography component="span">
                        {auctionData.manufacturer}
                    </Typography>

                    <Typography component="span" variant="subtitle1" sx={{ fontWeight: 'bold' }}>
                        Description:&nbsp;
                    </Typography>
                    <Typography component="span">
                        {auctionData.description}
                    </Typography>

                    <Typography component="span" variant="subtitle1" sx={{ fontWeight: 'bold', marginTop: '30px'}}>
                        Auction start date:&nbsp;
                    </Typography>
                    <Typography component="span">
                        {startDateLocal}
                    </Typography>

                    <Typography component="span" variant="subtitle1" sx={{ fontWeight: 'bold' }}>
                        Auction end date:&nbsp;
                    </Typography>
                    <Typography component="span">
                        {endDateLocal}
                    </Typography>
                </Box>
            </Box>
        </Container>
    );
};

export default AuctionInfo;
