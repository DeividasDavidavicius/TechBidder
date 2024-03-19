import axios from "axios";
import { API_URL } from "../utils/Constants";

export const getParts = async (partCategoryId) => {
    try {
      const response = await axios.get(`${API_URL}/categories/${partCategoryId}/parts`);
      return response.data;
    } catch (error) {
      console.error('Error getting parts:', error);
      throw error;
    }
  };
