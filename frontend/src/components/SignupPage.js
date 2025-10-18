import React, { useState } from "react";
import axios from "axios";
import { useNavigate } from "react-router-dom";

function SignupPage() {
  const [formData, setFormData] = useState({
    name: "",
    email: "",
    password: "",
    confirmPassword: "",
    gender: "",
  });
  const [message, setMessage] = useState("");
  const [isLoading, setIsLoading] = useState(false);
  const navigate = useNavigate();

  const handleChange = (e) => {
    setFormData({
      ...formData,
      [e.target.name]: e.target.value,
    });
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setIsLoading(true);
    setMessage("");

    // Validation
    if (formData.password !== formData.confirmPassword) {
      setMessage("Passwords do not match");
      setIsLoading(false);
      return;
    }

    if (formData.password.length < 6) {
      setMessage("Password must be at least 6 characters");
      setIsLoading(false);
      return;
    }

    try {
      const res = await axios.post(
        "http://localhost:5097/api/auth/signup",
        {
          name: formData.name,
          email: formData.email,
          password: formData.password,
          gender: formData.gender,
        },
        {
          withCredentials: true, // Important for session cookies
        }
      );

      setMessage(`Welcome ${res.data.name}! Account created successfully.`);

      // Store user info in localStorage for immediate access
      localStorage.setItem("userId", String(res.data.userId));
      localStorage.setItem("name", res.data.name || "");
      localStorage.setItem("email", res.data.email || "");

      // Dispatch custom event to update navbar
      window.dispatchEvent(new CustomEvent("authStateChanged"));

      // Redirect to create profile page
      setTimeout(() => {
        navigate("/create-profile");
      }, 1500);
    } catch (err) {
      if (err.response?.status === 409) {
        setMessage(
          "Email already exists. Please use a different email or try logging in."
        );
      } else if (err.response?.status === 400) {
        setMessage(
          err.response.data.message || "Please fill in all required fields."
        );
      } else {
        setMessage("Failed to create account. Please try again.");
      }
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="container mt-5">
      <div className="row justify-content-center">
        <div className="col-md-6">
          <div className="card">
            <div className="card-body">
              <h2 className="card-title text-center mb-4">Create Account</h2>
              <form onSubmit={handleSubmit}>
                <div className="mb-3">
                  <label className="form-label">Full Name</label>
                  <input
                    type="text"
                    className="form-control"
                    name="name"
                    value={formData.name}
                    onChange={handleChange}
                    required
                  />
                </div>

                <div className="mb-3">
                  <label className="form-label">Email</label>
                  <input
                    type="email"
                    className="form-control"
                    name="email"
                    value={formData.email}
                    onChange={handleChange}
                    required
                  />
                </div>

                <div className="mb-3">
                  <label className="form-label">Gender</label>
                  <select
                    className="form-select"
                    name="gender"
                    value={formData.gender}
                    onChange={handleChange}
                    required
                  >
                    <option value="">Select Gender</option>
                    <option value="M">Male</option>
                    <option value="F">Female</option>
                  </select>
                </div>

                <div className="mb-3">
                  <label className="form-label">Password</label>
                  <input
                    type="password"
                    className="form-control"
                    name="password"
                    value={formData.password}
                    onChange={handleChange}
                    minLength="6"
                    required
                  />
                </div>

                <div className="mb-3">
                  <label className="form-label">Confirm Password</label>
                  <input
                    type="password"
                    className="form-control"
                    name="confirmPassword"
                    value={formData.confirmPassword}
                    onChange={handleChange}
                    required
                  />
                </div>

                <button
                  type="submit"
                  className="btn btn-primary w-100"
                  disabled={isLoading}
                >
                  {isLoading ? "Creating Account..." : "Create Account"}
                </button>
              </form>

              {message && (
                <div
                  className={`alert mt-3 ${
                    message.includes("Welcome")
                      ? "alert-success"
                      : "alert-danger"
                  }`}
                >
                  {message}
                </div>
              )}

              <div className="text-center mt-3">
                <p>
                  Already have an account? <a href="/login">Login here</a>
                </p>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}

export default SignupPage;
