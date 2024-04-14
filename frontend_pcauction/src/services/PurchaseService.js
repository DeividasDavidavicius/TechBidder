import axios from "axios";
import { API_URL } from "../utils/ApiConstant";

export const getPurchase = async (auctionId) => {
  try {
    const response = await axios.get(`${API_URL}/auctions/${auctionId}/purchases`);
    return response.data;
  } catch (error) {
    console.error('Error getting purchase', error);
    throw error;
  }
};

export const getStripePurchaseSession = async (auctionId) => {
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

export const patchPurchase = async (auctionId) => {
  try {
    const accessToken = localStorage.getItem('accessToken');
    await axios.patch(`${API_URL}/auctions/${auctionId}/purchases`, {}, {
      headers: {
        "Content-Type": "application/json",
        "Authorization": `Bearer ${accessToken}`
      }
    })
  } catch (error) {
    console.error("Error patching purchase", error);
    throw error;
  }
};
