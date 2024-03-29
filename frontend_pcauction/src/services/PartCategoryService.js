import axios from "axios";
import { API_URL } from "../utils/ApiConstant";

export const getCategory = async (partCategoryId) => {
  try {
    const response = await axios.get(`${API_URL}/categories/${partCategoryId}`);
    return response.data;
  } catch (error) {
    console.error('Error getting category', error);
    throw error;
  }
};

export const getCategories = async () => {
    try {
      const response = await axios.get(`${API_URL}/categories`);
      return response.data;
    } catch (error) {
      console.error('Error getting categories:', error);
      throw error;
    }
  };
