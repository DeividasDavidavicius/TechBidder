import AuctionInfo from "../components/auctions/AuctionInfo";
import CreateAuction from "../components/auctions/CreateAuction";
import MainPage from "../components/MainPage";
import Login from "../components/auth/Login";
import Register from "../components/auth/Register";
import PATHS from "./Paths";
import AuctionList from "../components/auctions/AuctionList";
import EditAuction from "../components/auctions/EditAuction";
import PartList from "../components/parts/PartList";
import CreatePart from "../components/parts/CreatePart";
import EditPart from "../components/parts/EditPart";
import SeriesList from "../components/series/SeriesList";
import CreateSeries from "../components/series/CreateSeries";
import EditSeries from "../components/series/EditSeries";
import PartRequestList from "../components/parts/PartRequestList";
import PartRequestsCreate from "../components/parts/PartRequestsCreate";
import PsuCalculator from "../components/parts/PsuCalculator";
import PcBuildGenerator from "../components/parts/PcBuildGenerator";
import CompatibilityCheck from "../components/parts/CompatibilityCheck";
import UserProfile from "../components/user/UserProfile";

const AppRoutes = [
    { path: PATHS.LOGIN, element: <Login/> },
    { path: PATHS.REGISTER, element: <Register/> },
    { path: PATHS.AUCTIONS, element: <AuctionList/> },
    { path: PATHS.AUCTIONINFO, element: <AuctionInfo/> },
    { path: PATHS.CREATEAUCTION, element: <CreateAuction/> },
    { path: PATHS.EDITAUCTION, element: <EditAuction/> },
    { path: PATHS.PARTS, element: <PartList/> },
    { path: PATHS.CREATEPART, element: <CreatePart/> },
    { path: PATHS.EDITPART, element: <EditPart/> },
    { path: PATHS.SERIES, element: <SeriesList/>},
    { path: PATHS.CREATESERIES, element: <CreateSeries/>},
    { path: PATHS.EDITSERIES, element: <EditSeries/> },
    { path: PATHS.PARTREQUESTS, element: <PartRequestList/> },
    { path: PATHS.PARTREQUESTCREATE, element: <PartRequestsCreate/> },
    { path: PATHS.PSUCALCULATOR, element: <PsuCalculator/> },
    { path: PATHS.PCBUILDGENERATOR, element: <PcBuildGenerator/> },
    { path: PATHS.COMPATIBILITYCHECK, element: <CompatibilityCheck/> },
    { path: PATHS.USERPROFILE, element: <UserProfile/> },
    { path: PATHS.ANY, element: <MainPage/> },
    { index: true, path: PATHS.MAIN, element: <MainPage/> }
];

export default AppRoutes;
