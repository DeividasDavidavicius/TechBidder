import { useLocation, useNavigate } from "react-router-dom";
import PATHS from "../utils/Paths";
import { useEffect } from "react";

function MainPage() {
    const location = useLocation();
    const navigation = useNavigate();

    useEffect(()=> {
        if(location.pathname !== PATHS.MAIN)
        {
            navigation(PATHS.MAIN);
        }
    });

    return (
        <div>
            <h2>Pc auctions</h2>
            <h4>Auction your pc parts now</h4>
        </div>
    );
}

export default MainPage;
