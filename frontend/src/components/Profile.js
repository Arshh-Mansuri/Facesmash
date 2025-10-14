import React, { useEffect, useState } from "react";
import axios from "axios";

const Profile = () => {
  const [user, setUser] = useState(null);
  const [bio, setBio] = useState("");
  const [file, setFile] = useState(null);

  // Fetch user data
  const fetchProfile = async () => {
    try {
      const res = await axios.get("http://localhost:5097/api/profile/1"); // Example: User ID 1
      setUser(res.data);
      setBio(res.data.bio || "");
    } catch (err) {
      console.error(err);
    }
  };

  // Update bio
  const handleSave = async () => {
    try {
      await axios.put("http://localhost:5097/api/profile/1", { bio });
      alert("Profile updated!");
      fetchProfile();
    } catch (err) {
      console.error(err);
    }
  };

  // Upload image
  const handleUpload = async (e) => {
    e.preventDefault();
    const formData = new FormData();
    formData.append("file", file);
    await axios.post("http://localhost:5097/api/upload", formData, {
      headers: { "Content-Type": "multipart/form-data" },
    });
    alert("Image uploaded!");
    fetchProfile();
  };

  useEffect(() => {
    fetchProfile();
  }, []);

  if (!user) return <p>Loading...</p>;

  return (
    <div className="profile-container text-center p-4">
      <h2>{user.name}</h2>
      <img
        src={user.photoUrl || "https://via.placeholder.com/150"}
        alt={user.name}
        className="rounded-circle mb-3"
        width="150"
      />
      <p>{user.email}</p>
      <textarea
        className="form-control mb-3"
        value={bio}
        onChange={(e) => setBio(e.target.value)}
      />
      <button className="btn btn-primary" onClick={handleSave}>
        Save Bio
      </button>

      <hr />
      <form onSubmit={handleUpload}>
        <input
          type="file"
          className="form-control mb-2"
          onChange={(e) => setFile(e.target.files[0])}
        />
        <button className="btn btn-success" type="submit">
          Upload Image
        </button>
      </form>
    </div>
  );
};

export default Profile;
