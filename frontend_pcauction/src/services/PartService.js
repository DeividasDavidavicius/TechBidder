import axios from "axios";
import { API_URL } from "../utils/ApiConstant";

export const getPart = async (partCategoryId, partId) => {
  try {
    const response = await axios.get(`${API_URL}/categories/${partCategoryId}/parts/${partId}`);
    return response.data;
  } catch (error) {
    console.error('Error getting part', error);
    throw error;
  }
};

export const getParts = async (partCategoryId) => {
    try {
      const response = await axios.get(`${API_URL}/categories/${partCategoryId}/parts`);
      return response.data;
    } catch (error) {
      console.error('Error getting parts:', error);
      throw error;
    }
  };

  export const postPart = async (data, partCategoryId) => {
    try {
      const accessToken = localStorage.getItem('accessToken');
      await axios.post(`${API_URL}/categories/${partCategoryId}/parts`, data, {
        headers: {
          "Content-Type": "application/json",
          "Authorization": `Bearer ${accessToken}`
        }
      })
    } catch (error) {
      console.error("Error posting part", error);
      throw error;
    }
  };

  export const patchPart = async (data, partCategoryId, partId) => {
    try {
      const accessToken = localStorage.getItem('accessToken');
      await axios.patch(`${API_URL}/categories/${partCategoryId}/parts/${partId}`, data, {
        headers: {
          "Content-Type": "application/json",
          "Authorization": `Bearer ${accessToken}`
        }
      })
    } catch (error) {
      console.error("Error updating part", error);
      throw error;
    }
  };

  export const deletePart = async (partCategoryId, partId) => {
    try {
      const accessToken = localStorage.getItem('accessToken');
      await axios.delete(`${API_URL}/categories/${partCategoryId}/parts/${partId}`, {
        headers: {
          "Content-Type": "application/json",
          "Authorization": `Bearer ${accessToken}`
        }
      })
    } catch (error) {
      console.error("Error deleting part", error);
      throw error;
    }
  };
