import axios from "axios";
import { API_URL } from "../utils/Constants";

export const getSeries = async (partCategoryId, seriesId) => {
  try {
    const response = await axios.get(`${API_URL}/categories/${partCategoryId}/series/${seriesId}`);
    return response.data;
  } catch (error) {
    console.error('Error getting series', error);
    throw error;
  }
};

export const getAllCategorySeries = async (partCategoryId) => {
    try {
      const response = await axios.get(`${API_URL}/categories/${partCategoryId}/series`);
      return response.data;
    } catch (error) {
      console.error('Error getting all category series:', error);
      throw error;
    }
  };

  export const postSeries = async (data, partCategoryId) => {
    try {
      const accessToken = localStorage.getItem('accessToken');
      await axios.post(`${API_URL}/categories/${partCategoryId}/series`, data, {
        headers: {
          "Content-Type": "application/json",
          "Authorization": `Bearer ${accessToken}`
        }
      })
    } catch (error) {
      console.error("Error posting series", error);
      throw error;
    }
  };

  export const putSeries = async (data, partCategoryId, seriesid) => {
    try {
      const accessToken = localStorage.getItem('accessToken');
      await axios.put(`${API_URL}/categories/${partCategoryId}/series/${seriesid}`, data, {
        headers: {
          "Content-Type": "application/json",
          "Authorization": `Bearer ${accessToken}`
        }
      })
    } catch (error) {
      console.error("Error updating series", error);
      throw error;
    }
  };

  export const deleteSeries = async (partCategoryId, seriesId) => {
    try {
      const accessToken = localStorage.getItem('accessToken');
      await axios.delete(`${API_URL}/categories/${partCategoryId}/series/${seriesId}`, {
        headers: {
          "Content-Type": "application/json",
          "Authorization": `Bearer ${accessToken}`
        }
      })
    } catch (error) {
      console.error("Error deleting series", error);
      throw error;
    }
  };
