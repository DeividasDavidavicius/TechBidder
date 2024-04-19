import axios from 'axios';
import { API_URL } from '../utils/ApiConstant';

export const postAuction = async (data) => {
    try {
      const accessToken = localStorage.getItem('accessToken');
      return await axios.post(`${API_URL}/auctions`, data, {
        headers: {
          "Authorization": `Bearer ${accessToken}`
        }
      })
    } catch (error) {
      console.error("Error posting auction", error);
      throw error;
    }
  };

  export const getAuction = async (id) => {
    try {
      const response = await axios.get(`${API_URL}/auctions/${id}`);
      return response.data;
    } catch (error) {
      console.error('Error getting auction:', error);
      throw error;
    }
  };

  export const getAuctionsWithPagination = async (page, categoryId, seriesId, partId, sortType) => {
    try {
      const response = await axios.get(`${API_URL}/auctions/pagination?page=${page}&categoryId=${categoryId}&seriesId=${seriesId}&partId=${partId}&sortType=${sortType}`
      );
      return response.data;
    } catch (error) {
      console.error('Error getting auction:', error);
      throw error;
    }
  };

  export const putAuction = async (data, id) => {
    try {
      const accessToken = localStorage.getItem('accessToken');
      await axios.put(`${API_URL}/auctions/${id}`, data, {
        headers: {
          "Authorization": `Bearer ${accessToken}`
        }
      })
    } catch (error) {
      console.error("Error updating auction", error);
      throw error;
    }
  };

  export const patchAuction = async (data, id) => {
    try {
      const accessToken = localStorage.getItem('accessToken');
      await axios.patch(`${API_URL}/auctions/${id}`, data, {
        headers: {
          "Authorization": `Bearer ${accessToken}`
        }
      })
    } catch (error) {
      console.error("Error updating auctions part", error);
      throw error;
    }
  };

  export const getAuctionRecommendations = async (id) => {
    try {
      const response = await axios.get(`${API_URL}/auctions/${id}/recommendations`);
      return response.data;
    } catch (error) {
      console.error('Error getting auction:', error);
      throw error;
    }
  };

  export const getUserNewAuctions = async () => {
    try {
      const accessToken = localStorage.getItem('accessToken');
      const response = await axios.get(`${API_URL}/user/newauctions`, {
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

  export const getUserActiveAuctions = async () => {
    try {
      const accessToken = localStorage.getItem('accessToken');
      const response = await axios.get(`${API_URL}/user/activeauctions`, {
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

  export const getUserEndedAuctions = async () => {
    try {
      const accessToken = localStorage.getItem('accessToken');
      const response = await axios.get(`${API_URL}/user/endedauctions`, {
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

  export const getUserWonAuctions = async () => {
    try {
      const accessToken = localStorage.getItem('accessToken');
      const response = await axios.get(`${API_URL}/user/wonauctions`, {
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

  export const cancelAuction = async (auctionId) => {
    try {
      const accessToken = localStorage.getItem('accessToken');
      await axios.patch(`${API_URL}/auctions/${auctionId}/cancel`, {}, {
        headers: {
          "Content-Type": "application/json",
          "Authorization": `Bearer ${accessToken}`
        }
      })
    } catch (error) {
      console.error("Error cancelling auction", error);
      throw error;
    }
  };
