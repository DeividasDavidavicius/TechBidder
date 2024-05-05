import axios from 'axios';
import { API_URL } from '../utils/ApiConstant';

export const postBid = async (data, auctionId) => {
    try {
      const accessToken = localStorage.getItem('accessToken');
      await axios.post(`${API_URL}/auctions/${auctionId}/bids`, data, {
        headers: {
          "Content-Type": "application/json",
          "Authorization": `Bearer ${accessToken}`
        }
      })
    } catch (error) {
      console.error("Error posting bid", error);
      throw error;
    }
};

export const getHighestBid = async (auctionId) => {
  try {
    const response = await axios.get(`${API_URL}/auctions/${auctionId}/bids/highest`);
    return response.data;
  } catch (error) {
    console.error('Error getting highest auction bid', error);
    throw error;
  }
};

export const getAuctionBids = async (auctionId) => {
  try {
    const response = await axios.get(`${API_URL}/auctions/${auctionId}/bids`);
    return response.data;
  } catch (error) {
    console.error('Error getting auction bids', error);
    throw error;
  }
};

export const deleteBid = async (auctionId, bidId) => {
  try {
    const accessToken = localStorage.getItem('accessToken');
    await axios.delete(`${API_URL}/auctions/${auctionId}/bids/${bidId}`, {
      headers: {
        "Content-Type": "application/json",
        "Authorization": `Bearer ${accessToken}`
      }
    })
  } catch (error) {
    console.error("Error deleting bid", error);
    throw error;
  }
};

export const getUserBids = async () => {
  try {
    const accessToken = localStorage.getItem('accessToken');
    const response = await axios.get(`${API_URL}/user/bids`, {
      headers: {
        "Content-Type": "application/json",
        "Authorization": `Bearer ${accessToken}`
      }
    });

    return response.data;
  } catch (error) {
    console.error("Error getting user bids", error);
    throw error;
  }
};

export const getWinningUserBids = async () => {
  try {
    const accessToken = localStorage.getItem('accessToken');
    const response = await axios.get(`${API_URL}/user/winningBids`, {
      headers: {
        "Content-Type": "application/json",
        "Authorization": `Bearer ${accessToken}`
      }
    });

    return response.data;
  } catch (error) {
    console.error("Error getting winning user bids", error);
    throw error;
  }
};
