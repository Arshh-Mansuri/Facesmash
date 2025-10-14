import React, { useEffect, useState } from "react";
import axios from "axios";

const Leaderboard = () => {
  const [users, setUsers] = useState([]);

  useEffect(() => {
    const fetchLeaderboard = async () => {
      try {
        const res = await axios.get("http://localhost:5097/api/leaderboard");
        setUsers(res.data);
      } catch (err) {
        console.error(err);
      }
    };
    fetchLeaderboard();
  }, []);

  return (
    <div className="container mt-4">
      <h2 className="text-center mb-4">🏆 Top 100 Leaderboard</h2>
      <table className="table table-striped table-hover">
        <thead>
          <tr>
            <th>Rank</th>
            <th>Photo</th>
            <th>Name</th>
            <th>Bio</th>
            <th>Rating</th>
          </tr>
        </thead>
        <tbody>
          {users.map((user, index) => (
            <tr key={user.id}>
              <td>{index + 1}</td>
              <td>
                <img
                  src={user.photoUrl}
                  alt={user.name}
                  style={{ width: "50px", borderRadius: "50%" }}
                />
              </td>
              <td>{user.name}</td>
              <td>{user.bio || "No bio yet"}</td>
              <td>{user.rating}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
};

export default Leaderboard;
