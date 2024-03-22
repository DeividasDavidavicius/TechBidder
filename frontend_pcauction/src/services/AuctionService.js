import axios from 'axios';
import { API_URL } from '../utils/Constants';

export const postAuction = async (data) => {
    try {
      const accessToken = localStorage.getItem('accessToken');
      await axios.post(`${API_URL}/auctions`, data, {
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

  export const getAuctionsWithPagination = async (page) => {
    try {
      const response = await axios.get(`${API_URL}/auctions/pagination?page=${page}`);
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
