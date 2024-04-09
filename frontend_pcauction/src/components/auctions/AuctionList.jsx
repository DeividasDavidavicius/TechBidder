import { useEffect, useState } from "react";
import { useLocation, useNavigate } from "react-router-dom";
import PATHS from "../../utils/Paths";
import { Autocomplete, Avatar, Box, Card, CardActionArea, CardContent, CardHeader, Container, CssBaseline, FormControl, Grid, Pagination, TextField, Typography } from "@mui/material";
import { getAuctionsWithPagination } from "../../services/AuctionService";
import { timeLeft } from "../../utils/DateUtils";
import { getHighestBid } from "../../services/BIdService";
import { getCategories } from "../../services/PartCategoryService";
import { getParts } from "../../services/PartService";
import { getAllCategorySeries } from "../../services/SeriesService";

function AuctionList() {
    const navigate = useNavigate();

    const location = useLocation();
    const queryParams = new URLSearchParams(location.search);

    const [currentPage, setCurrentPage] = useState(parseInt(queryParams.get('page', 1) || 1));
    const [categoryId, setCategoryId] = useState(queryParams.get('categoryId'));
    const [seriesId, setSeriesId] = useState(queryParams.get('seriesId'));
    const [partId, setPartId] = useState(queryParams.get('partId'));
    const [totalAuctions, setTotalAuctions] = useState(0);
    const [auctions, setAuctions] = useState([]);
    const auctionsPerPage = 5;

    const [categories, setCategories] = useState([]);
    const [series, setSeries] = useState([]);
    const [parts, setParts] = useState([]);

    const [selectedCategory, setSelectedCategory] = useState(null);
    const [selectedSeries, setSelectedSeries] = useState(null);
    const [selectedPart, setSelectedPart] = useState(null);

    useEffect(() => {
        const fetchAuctionsData = async () => {
            const result = await getAuctionsWithPagination(currentPage, selectedCategory ? selectedCategory.id : (categoryId ? categoryId : ""),
                                                           selectedSeries ? selectedSeries.id: (seriesId ? seriesId : ""), selectedPart ? selectedPart.id : (partId ? partId : ""));
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


        const fetchFiltersData = async () => {
          const categoriesResult = await getCategories();
          if (categoryId !== null) {
            const category = categoriesResult.find(category => category.id === categoryId);
            if (category) {
              setSelectedCategory({ id: category.id, label: category.id });
            }
          }
          setCategoryId(null);
          setCategories(categoriesResult);

          const seriesPromises = categoriesResult.map(category =>
            getAllCategorySeries(category.id)
          );

          const seriesData = await Promise.all(seriesPromises);
          const flattenedSeriesData = seriesData.flat();
          if (seriesId !== null) {
            const series = flattenedSeriesData.find(series => series.id === seriesId);
            if (series) {
              setSelectedSeries({ id: series.id, label: series.name });
            }
          }
          setSeriesId(null);
          setSeries(flattenedSeriesData);

          const partsPromises = categoriesResult.map(category =>
            getParts(category.id)
          );

          const partsData = await Promise.all(partsPromises);
          const flattenedPartData = partsData.flat();
          if (partId !== null) {
            const part = flattenedPartData.find(part => part.id === partId);
            if (part) {
              setSelectedPart({ id: part.id, label: part.name });
            }
          }
          setPartId(null);
          setParts(flattenedPartData);
      };

        fetchAuctionsData();
        fetchFiltersData();

    }, [currentPage, selectedCategory, selectedSeries, selectedPart]);

    const handlePageChange = (event, newPage) => {
      const queryParams = [];
      if (newPage) {
          queryParams.push(`page=${newPage}`);
      }
      if (selectedCategory?.id) {
          queryParams.push(`categoryId=${selectedCategory.id}`);
      }
      if (selectedSeries?.id) {
          queryParams.push(`seriesId=${selectedSeries.id}`);
      }
      if (selectedPart?.id) {
          queryParams.push(`partId=${selectedPart.id}`);
      }
      navigate(PATHS.AUCTIONS + "?" + queryParams.join("&"));
      setCurrentPage(newPage);
    };

    const truncateText = (text, maxLength) => {
        if (text.length <= maxLength) return text;
        return text.slice(0, maxLength).trimEnd() + '...';
    };

    const handleCardClick = (auctionId) => {
        navigate(PATHS.AUCTIONINFO.replace(":auctionId", auctionId));
    };

    const handleCategoryChange = async (category) => {
        await setSelectedCategory(category);
        await setSelectedSeries(null);
        await setSelectedPart(null);

        const queryParams = [];
        if (currentPage) {
            queryParams.push(`page=${currentPage}`);
        }
        if (category?.id) {
            queryParams.push(`categoryId=${category.id}`);
        }
        navigate(PATHS.AUCTIONS + "?" + queryParams.join("&"));
    }

    const handleSeriesChange = async (series) => {
      await setSelectedSeries(series);
      await setSelectedPart(null);

      const queryParams = [];
      if (currentPage) {
          queryParams.push(`page=${currentPage}`);
      }
      if (selectedCategory?.id) {
          queryParams.push(`categoryId=${selectedCategory.id}`);
      }
      if (series?.id) {
          queryParams.push(`seriesId=${series.id}`);
      }
      navigate(PATHS.AUCTIONS + "?" + queryParams.join("&"));
    }

  const handlePartChange = async (part) => {
    await setSelectedPart(part);

    const queryParams = [];
    if (currentPage) {
        queryParams.push(`page=${currentPage}`);
    }
    if (selectedCategory?.id) {
        queryParams.push(`categoryId=${selectedCategory.id}`);
    }
    if (selectedSeries?.id) {
        queryParams.push(`seriesId=${selectedSeries.id}`);
    }
    if (part?.id) {
        queryParams.push(`partId=${part.id}`);
    }
    navigate(PATHS.AUCTIONS + "?" + queryParams.join("&"));
  }

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
        <Grid container spacing={2} sx={{marginBottom: 2}}>
            <Grid item xs={12} sm={4}>
                <FormControl fullWidth>
                    <Autocomplete
                        id="categories-autocomplete"
                        options={categories.map(p => ({ id: p.id, label: p.id }))}
                        getOptionLabel={(option) => option?.label || option}
                        value={selectedCategory}
                        onChange={(event, newValue) => handleCategoryChange(newValue)}
                        required
                        renderInput={(params) => <TextField {...params} label="Part category"/>}
                        isOptionEqualToValue={(option, value) => true}
                    />
                </FormControl>
            </Grid>
            <Grid item xs={12} sm={4}>
                <FormControl fullWidth>
                    <Autocomplete
                        id="series-autocomplete"
                        options={selectedCategory ? series.filter(s => s.categoryId === selectedCategory?.id)
                          .map(s => ({ id: s.id, label: s.name }))
                          : series.map(s => ({ id: s.id, label: s.name }))}
                        getOptionLabel={(option) => option?.label || option}
                        value={selectedSeries}
                        onChange={(event, newValue) => handleSeriesChange(newValue)}
                        required
                        renderInput={(params) => <TextField {...params} label="Series"/>}
                        isOptionEqualToValue={(option, value) => true}
                    />
                </FormControl>
            </Grid>
            <Grid item xs={12} sm={4}>
                <FormControl fullWidth>
                    <Autocomplete
                        id="parts-autocomplete"
                        options={(selectedCategory && selectedSeries) ?
                          parts.filter(p => p.categoryId === selectedCategory.id && p.seriesId === selectedSeries.id)
                               .map(p => ({ id: p.id, label: p.name })) :
                          (selectedCategory ?
                           parts.filter(p => p.categoryId === selectedCategory.id)
                                .map(p => ({ id: p.id, label: p.name })) :
                           (selectedSeries ?
                            parts.filter(p => p.seriesId === selectedSeries.id)
                                 .map(p => ({ id: p.id, label: p.name })) :
                            parts.map(p => ({ id: p.id, label: p.name }))))
                        }
                        getOptionLabel={(option) => option?.label || option}
                        value={selectedPart}
                        onChange={(event, newValue) => handlePartChange(newValue)}
                        required
                        renderInput={(params) => <TextField {...params} label="Part"/>}
                        isOptionEqualToValue={(option, value) => true}
                    />
                </FormControl>
            </Grid>
        </Grid>

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
