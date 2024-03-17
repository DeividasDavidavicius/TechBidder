import AuctionInfo from "../components/auctions/AuctionInfo";
import CreateAuction from "../components/auctions/Create";
import MainPage from "../components/MainPage";
import Login from "../components/auth/Login";
import Register from "../components/auth/Register";
import PATHS from "./Paths";
import AuctionList from "../components/auctions/AuctionList";
import EditAuction from "../components/auctions/Edit";


const AppRoutes = [
    { index: true, path: PATHS.MAIN, element: <MainPage/> },
    { path: PATHS.LOGIN, element: <Login/> },
    { path: PATHS.REGISTER, element: <Register/> },
    { path: PATHS.ANY, element: <MainPage/> },
    { path: PATHS.AUCTIONS, element: <AuctionList/> },
    { path: PATHS.AUCTIONINFO, element: <AuctionInfo/> },
    { path: PATHS.CREATEAUCTION, element: <CreateAuction/> },
    { path: PATHS.EDITAUCTION, element: <EditAuction/> }
];

export default AppRoutes;
