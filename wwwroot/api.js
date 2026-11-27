// API client for the Courses page
// Base URL for the API – same domain, same port
const API_BASE = '/api/courses';

/** Load all courses and populate the table */
async function loadCourses() {
  try {
    const res = await fetch(API_BASE);
    if (!res.ok) throw new Error(`Failed to load courses: ${res.status}`);
    const courses = await res.json();
    populateTable(courses);
  } catch (err) {
    console.error(err);
    alert(err.message);
  }
}

/** Render a table of courses */
function populateTable(courses) {
  const tbody = document.getElementById('coursesTable');
  tbody.innerHTML = '';
  courses.forEach(c => {
    const tr = document.createElement('tr');
    tr.innerHTML = `
      <td>${c.id}</td>
      <td>${c.code}</td>
      <td>${c.title}</td>
      <td>${c.subject}</td>
      <td>${c.credits}</td>
      <td class="actions">
        <button class="btn-edit" onclick="editCourse(${c.id})">Edit</button>
        <button class="btn-delete" onclick="deleteCourse(${c.id})">Delete</button>
      </td>`;
    tbody.appendChild(tr);
  });
}

/** Read form values into a DTO */
function getFormValues() {
  return {
    code: document.getElementById('courseCode').value.trim(),
    title: document.getElementById('courseTitle').value.trim(),
    subject: document.getElementById('courseSubject').value.trim(),
    credits: parseFloat(document.getElementById('courseCredits').value),
  };
}

/** Simple validation */
function validate(values) {
  if (!values.code || !values.title || !values.subject || isNaN(values.credits)) {
    alert('All fields are required.');
    return false;
  }
  if (values.credits < 0) {
    alert('Credits must be non‑negative.');
    return false;
  }
  return true;
}

/** Save (create or update) a course */
async function saveCourse() {
  const id = document.getElementById('courseId').value;
  const dto = getFormValues();
  if (!validate(dto)) return;

  try {
    const method = id ? 'PUT' : 'POST';
    const url = id ? `${API_BASE}/${id}` : API_BASE;
    const res = await fetch(url, {
      method,
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(dto),
    });
    if (!res.ok) {
      const err = await res.json();
      throw new Error(err.error || `Error ${res.status}`);
    }
    const saved = await res.json();
    alert(`Course ${id ? 'updated' : 'created'} successfully.`);
    resetForm();
    loadCourses();
  } catch (err) {
    console.error(err);
    alert(err.message);
  }
}

/** Load a single course for editing */
async function editCourse(id) {
  try {
    const res = await fetch(`${API_BASE}/${id}`);
    if (!res.ok) throw new Error(`Course not found`);
    const c = await res.json();
    document.getElementById('courseId').value = c.id;
    document.getElementById('courseCode').value = c.code;
    document.getElementById('courseTitle').value = c.title;
    document.getElementById('courseSubject').value = c.subject;
    document.getElementById('courseCredits').value = c.credits;
    document.getElementById('formTitle').textContent = 'Edit Course';
  } catch (err) {
    console.error(err);
    alert(err.message);
  }
}

/** Delete a course */
async function deleteCourse(id) {
  if (!confirm('Delete this course?')) return;
  try {
    const res = await fetch(`${API_BASE}/${id}`, { method: 'DELETE' });
    if (!res.ok) throw new Error(`Failed to delete: ${res.status}`);
    alert('Course deleted.');
    loadCourses();
  } catch (err) {
    console.error(err);
    alert(err.message);
  }
}

/** Reset form to add mode */
function resetForm() {
  document.getElementById('courseId').value = '';
  document.getElementById('courseCode').value = '';
  document.getElementById('courseTitle').value = '';
  document.getElementById('courseSubject').value = '';
  document.getElementById('courseCredits').value = '';
  document.getElementById('formTitle').textContent = 'Add Course';
}

// Load courses on page load
window.addEventListener('DOMContentLoaded', loadCourses);
