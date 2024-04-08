import axios from "axios";
import { API_URL } from "../utils/ApiConstant";

export const calculatePsu = async(data) => {
    try {
      const response = await axios.post(`${API_URL}/psucalculator`, data, {
        headers: {
          "Content-Type": "application/json"
        }
      })
      return response;
    } catch (error) {
      console.error("Failed to calculate PSU", error);
      throw error;
    }
  };


  export const generatePcBuild = async(data) => {
    try {
      const response = await axios.post(`${API_URL}/pcbuildgenerator`, data, {
        headers: {
          "Content-Type": "application/json"
        }
      })
      return response.data;
    } catch (error) {
      console.error("Failed to generate PC build", error);
      throw error;
    }
  };
