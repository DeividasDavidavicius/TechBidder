# TechBidder - Computer Hardware Auction Information System

## Summary

**TechBidder** is a web-based system designed to help users buy and sell components for desktop computers through an auction platform. This system combines various functionalities into one cohesive platform, allowing users to easily find compatible parts, view auction recommendations, and access unique features like a PSU calculator and PC builder tool.

---

## Features

### For Users:
- **Browse Auctions:** Users can explore auctions for various computer components.
- **Auction Recommendations:** Get recommendations based on average prices to help find the right parts.
- **Part Compatibility:** Use the compatibility checker to verify if parts fit together.
- **PSU Calculator:** A feature to calculate the required PSU for your build.
- **PC Builder:** Build your custom computer within a set budget by selecting compatible parts.

### For Administrators:
- **Manage Parts:** Admins can add, edit, or delete parts in the system.
- **Manage Parts Series:** Admins can control component series to ensure correct categorization.
- **Request Management:** Admins handle user requests for auction updates or issues.

### System Architecture:
- **Server-Side:** Built using the .NET 7.0.18 framework and C# 11.0.
- **Client-Side:** Developed using JavaScript and React 18.3.1.
- **Database:** Microsoft SQL Server 15.0.4153 and Entity Framework Core 7.0.16.
- **Third-Party Integrations:** 
  - **Azure Blob Storage:** For storing auction images.
  - **Stripe:** For payment processing.
  - **RapidAPI:** For fetching average component prices from eBay.

---
