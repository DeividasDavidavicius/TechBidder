import MainPage from "../components/MainPage";
import AuctionInfo from "../components/auctions/AuctionInfo";
import CreateAuction from "../components/auctions/Create";
import Login from "../components/auth/Login";
import Register from "../components/auth/Register";
import PATHS from "./Paths";


const AppRoutes = [
    { index: true, path: PATHS.MAIN, element: <MainPage/> },
    { path: PATHS.LOGIN, element: <Login/> },
    { path: PATHS.REGISTER, element: <Register/> },
    { path: PATHS.ANY, element: <MainPage/> },
    { path: PATHS.AUCTIONINFO, element: <AuctionInfo/> },
    { path: PATHS.CREATEAUCTION, element: <CreateAuction/> }
];

export default AppRoutes;
