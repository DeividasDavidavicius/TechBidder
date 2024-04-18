import axios from "axios";
import { API_URL } from "../utils/ApiConstant";

export const getUserData = async (userId) => {
  try {
    const response = await axios.get(`${API_URL}/user/${userId}`);
    return response.data;
  } catch (error) {
    console.error('Error getting user data', error);
    throw error;
  }
};

export const patchUserData = async (data) => {
    try {
    const accessToken = localStorage.getItem('accessToken');
    await axios.patch(`${API_URL}/user`, data, {
        headers: {
            "Content-Type": "application/json",
            "Authorization": `Bearer ${accessToken}`
        }
    })
    } catch (error) {
        console.error("Error updating user data", error);
        throw error;
    }
};

