import axios from "axios";
import { API_URL } from "../utils/ApiConstant";

export const postStripePurchase = async (auctionId) => {
    try {
      const accessToken = localStorage.getItem('accessToken');
      const response = await axios.get(`${API_URL}/auctions/${auctionId}/purchases/stripe`, {
        headers: {
          "Content-Type": "application/json",
          "Authorization": `Bearer ${accessToken}`
        }
      });

      return response.data;
    } catch (error) {
      console.error("Error getting stripe info", error);
      throw error;
    }
  };
