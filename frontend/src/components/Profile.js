import React, { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import axios from "axios";

const Profile = () => {
  const [user, setUser] = useState(null);
  const [bio, setBio] = useState("");
  const [name, setName] = useState("");
  const [photoUrl, setPhotoUrl] = useState("");
  const [file, setFile] = useState(null);
  const [message, setMessage] = useState("");
  const [isLoading, setIsLoading] = useState(false);

  const navigate = useNavigate();

  // Fetch user data using session-based auth
  const fetchProfile = async () => {
    try {
      const res = await axios.get("http://localhost:5097/api/profile/me", {
        withCredentials: true, // Important for session cookies
      });
      setUser(res.data);
      setBio(res.data.bio || "");
      setName(res.data.name || "");
      setPhotoUrl(res.data.photoUrl || "");
    } catch (err) {
      if (err.response?.status === 401) {
        setMessage("Please log in to view your profile.");
        setTimeout(() => {
          navigate("/login");
        }, 2000);
      } else {
        console.error(err);
        setMessage("Failed to load profile.");
      }
    }
  };

  // Update profile
  const handleSave = async () => {
    setIsLoading(true);
    setMessage("");

    try {
      await axios.put(
        "http://localhost:5097/api/profile/me",
        {
          name: name,
          bio: bio,
          photoUrl: photoUrl,
        },
        {
          withCredentials: true, // Important for session cookies
        }
      );

      setMessage("Profile updated successfully!");
      fetchProfile(); // Refresh profile data
    } catch (err) {
      if (err.response?.status === 401) {
        setMessage("Please log in to update your profile.");
        setTimeout(() => {
          navigate("/login");
        }, 2000);
      } else {
        setMessage("Failed to update profile. Please try again.");
      }
    } finally {
      setIsLoading(false);
    }
  };

  // Upload image (if backend supports it)
  const handleUpload = async (e) => {
    e.preventDefault();
    if (!file) {
      setMessage("Please select a file to upload.");
      return;
    }

    setIsLoading(true);
    setMessage("");

    const formData = new FormData();
    formData.append("file", file);

    try {
      await axios.post(
        "http://localhost:5097/api/profile/upload-photo",
        formData,
        {
          headers: { "Content-Type": "multipart/form-data" },
          withCredentials: true,
        }
      );

      setMessage("Image uploaded successfully!");
      fetchProfile(); // Refresh profile data
    } catch (err) {
      if (err.response?.status === 401) {
        setMessage("Please log in to upload images.");
        setTimeout(() => {
          navigate("/login");
        }, 2000);
      } else {
        setMessage("Failed to upload image. Please try again.");
      }
    } finally {
      setIsLoading(false);
    }
  };

  const handleLogout = async () => {
    try {
      await axios.post(
        "http://localhost:5097/api/auth/logout",
        {},
        {
          withCredentials: true,
        }
      );
    } catch (err) {
      console.error("Logout error:", err);
    } finally {
      // Clear localStorage regardless of API response
      localStorage.removeItem("userId");
      localStorage.removeItem("name");
      localStorage.removeItem("email");
      navigate("/login");
    }
  };

  useEffect(() => {
    fetchProfile();
  }, []);

  if (!user) return <p>Loading...</p>;

  return (
    <div className="profile-container text-center p-4">
      <div className="row justify-content-center">
        <div className="col-md-8">
          <div className="card">
            <div className="card-body">
              <h2 className="card-title">{user.name}</h2>

              <div className="position-relative d-inline-block mb-3">
                <img
                  src={user.photoUrl || "https://via.placeholder.com/150"}
                  alt={user.name}
                  className="rounded-circle"
                  width="150"
                  height="150"
                  onError={(e) => {
                    e.target.src = "https://via.placeholder.com/150";
                  }}
                />
                {user.photoUrl && user.photoUrl.includes("placeholder") && (
                  <div className="position-absolute top-0 start-50 translate-middle-x">
                    <span className="badge bg-warning text-dark">
                      Upload Photo
                    </span>
                  </div>
                )}
              </div>

              <p className="text-muted">{user.email}</p>
              <p className="badge bg-primary">Rating: {user.rating}</p>

              {message && (
                <div
                  className={`alert ${
                    message.includes("successfully")
                      ? "alert-success"
                      : "alert-danger"
                  }`}
                >
                  {message}
                </div>
              )}

              <hr />

              <div className="mb-3">
                <label className="form-label">Display Name</label>
                <input
                  type="text"
                  className="form-control"
                  value={name}
                  onChange={(e) => setName(e.target.value)}
                  placeholder="Enter your display name"
                />
              </div>

              <div className="mb-3">
                <label className="form-label">Bio</label>
                <textarea
                  className="form-control"
                  value={bio}
                  onChange={(e) => setBio(e.target.value)}
                  rows="4"
                  placeholder="Tell us about yourself..."
                  maxLength="500"
                />
                <div className="form-text">{bio.length}/500 characters</div>
              </div>

              <div className="mb-3">
                <label className="form-label">Photo URL</label>
                <input
                  type="url"
                  className="form-control"
                  value={photoUrl}
                  onChange={(e) => setPhotoUrl(e.target.value)}
                  placeholder="https://example.com/your-photo.jpg"
                />
              </div>

              <div className="d-flex gap-2 justify-content-center">
                <button
                  className="btn btn-primary"
                  onClick={handleSave}
                  disabled={isLoading}
                >
                  {isLoading ? "Saving..." : "Save Changes"}
                </button>
                <button
                  className="btn btn-outline-danger"
                  onClick={handleLogout}
                  disabled={isLoading}
                >
                  Logout
                </button>
              </div>

              <hr />

              <h5>Upload Image</h5>
              <form onSubmit={handleUpload}>
                <input
                  type="file"
                  className="form-control mb-2"
                  onChange={(e) => setFile(e.target.files[0])}
                  accept="image/*"
                />
                <button
                  className="btn btn-success"
                  type="submit"
                  disabled={isLoading}
                >
                  {isLoading ? "Uploading..." : "Upload Image"}
                </button>
              </form>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default Profile;
