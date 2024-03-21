import AuctionInfo from "../components/auctions/AuctionInfo";
import CreateAuction from "../components/auctions/Create";
import MainPage from "../components/MainPage";
import Login from "../components/auth/Login";
import Register from "../components/auth/Register";
import PATHS from "./Paths";
import AuctionList from "../components/auctions/AuctionList";
import EditAuction from "../components/auctions/Edit";
import PartList from "../components/parts/PartList";
import CreatePart from "../components/parts/CreatePart";
import EditPart from "../components/parts/EditPart";
import SeriesList from "../components/series/SeriesList";
import CreateSeries from "../components/series/CreateSeries";
import EditSeries from "../components/series/EditSeries";


const AppRoutes = [
    { index: true, path: PATHS.MAIN, element: <MainPage/> },
    { path: PATHS.LOGIN, element: <Login/> },
    { path: PATHS.REGISTER, element: <Register/> },
    { path: PATHS.ANY, element: <MainPage/> },
    { path: PATHS.AUCTIONS, element: <AuctionList/> },
    { path: PATHS.AUCTIONINFO, element: <AuctionInfo/> },
    { path: PATHS.CREATEAUCTION, element: <CreateAuction/> },
    { path: PATHS.EDITAUCTION, element: <EditAuction/> },
    { path: PATHS.PARTS, element: <PartList/> },
    { path: PATHS.CREATEPART, element: <CreatePart/> },
    { path: PATHS.EDITPART, element: <EditPart/> },
    { path: PATHS.SERIES, element: <SeriesList/>},
    { path: PATHS.CREATESERIES, element: <CreateSeries/>},
    { path: PATHS.EDITSERIES, element: <EditSeries/> }
];

export default AppRoutes;
