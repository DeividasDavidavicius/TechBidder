import axios from "axios";
import { API_URL } from "../utils/ApiConstant";

export const getPartRequest = async (partRequestId) => {
  try {
    const accessToken = localStorage.getItem('accessToken');
    const response = await axios.get(`${API_URL}/partrequests/${partRequestId}`, {
        headers: {
            "Content-Type": "application/json",
            "Authorization": `Bearer ${accessToken}`
        }
    });
    return response.data;
  } catch (error) {
    console.error('Error getting part request', error);
    throw error;
  }
};

export const getAllPartRequests = async () => {
    try {
      const accessToken = localStorage.getItem('accessToken');
      const response = await axios.get(`${API_URL}/partrequests`, {
        headers: {
            "Content-Type": "application/json",
            "Authorization": `Bearer ${accessToken}`
        }
      });
      return response.data;
    } catch (error) {
      console.error('Error getting all part requests', error);
      throw error;
    }
  };

  export const postPartRequest = async (data) => {
    try {
      const accessToken = localStorage.getItem('accessToken');
      await axios.post(`${API_URL}/partrequests`, data, {
        headers: {
          "Content-Type": "application/json",
          "Authorization": `Bearer ${accessToken}`
        }
      })
    } catch (error) {
      console.error("Error posting part request", error);
      throw error;
    }
  };

  export const deletePartRequest = async (partRequestId) => {
    try {
      const accessToken = localStorage.getItem('accessToken');
      await axios.delete(`${API_URL}/partrequests/${partRequestId}`, {
        headers: {
          "Content-Type": "application/json",
          "Authorization": `Bearer ${accessToken}`
        }
      })
    } catch (error) {
      console.error("Error deleting part request", error);
      throw error;
    }
  };
