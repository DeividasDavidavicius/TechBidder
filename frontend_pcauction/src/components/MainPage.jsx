import { useLocation, useNavigate } from "react-router-dom";
import PATHS from "../utils/Paths";
import { useEffect } from "react";
import { Box, Card, CardActionArea, CardContent, CardMedia, Container, CssBaseline, Grid, Typography } from "@mui/material";

function MainPage() {
    const location = useLocation();
    const navigation = useNavigate();

    const cardData = [
        { id: 1, imageUrl: '/assets/CPU.png', text: 'CPU', link: PATHS.AUCTIONS + "?categoryId=CPU" },
        { id: 2, imageUrl: '/assets/GPU.png', text: 'GPU', link: PATHS.AUCTIONS + "?categoryId=GPU" },
        { id: 3, imageUrl: '/assets/RAM.png', text: 'RAM', link: PATHS.AUCTIONS + "?categoryId=RAM" },
        { id: 4, imageUrl: '/assets/HDD.png', text: 'HDD', link: PATHS.AUCTIONS + "?categoryId=HDD" },
        { id: 5, imageUrl: '/assets/SSD.png', text: 'SSD', link: PATHS.AUCTIONS + "?categoryId=SSD" },
        { id: 6, imageUrl: '/assets/PSU.png', text: 'PSU', link: PATHS.AUCTIONS + "?categoryId=PSU" },
        { id: 7, imageUrl: '/assets/Motherboard.png', text: 'Mothherboard', link: PATHS.AUCTIONS + "?categoryId=Motherboard" },
      ];

      const handleCardClick = (link) => {
        // Redirect to the specified link
        window.location.href = link;
      };

    useEffect(()=> {
        if(location.pathname !== PATHS.MAIN)
        {
            navigation(PATHS.MAIN);
        }
    });

    return (
        <Container component="main" maxWidth="lg">
            <CssBaseline />
            <Box>
                <Box sx={{ marginTop: 2, marginBottom: 3, display: 'flex', flexDirection: 'column', alignItems: 'center' }}>
                    <Typography component="h1" variant="h5" sx={{ fontSize: '26px', fontWeight: 'bold', fontFamily: 'Arial, sans-serif', color: '#0d6267' }}>
                        CATEGORIES
                    </Typography>
                </Box>
                <Grid container spacing={3}>
                    {cardData.map((card) => (
                    <Grid item xs={12} sm={6} md={4} key={card.id}>
                        <Card>
                            <CardActionArea onClick={() => handleCardClick(card.link)}>
                                <CardMedia
                                component="img"
                                height="220"
                                image={card.imageUrl}
                                alt={`Image ${card.id}`}
                                style={{ objectFit: 'cover' }}
                                />
                                <CardContent>
                                    <Typography gutterBottom variant="h5" component="div">
                                    {card.text}
                                    </Typography>
                                </CardContent>
                            </CardActionArea>
                        </Card>
                    </Grid>
                    ))}
                </Grid>
            </Box>
      </Container>
    );
}

export default MainPage;
