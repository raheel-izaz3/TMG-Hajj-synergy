function addAddress() {
    let rootNode = document.getElementById("addresses");
    let id = Date.now() + Math.floor(Math.random() * 10000)
    let row = document.createElement("DIV");
    row.classList.add("row");
    row.setAttribute("id", "address_" + id);

    rootNode.appendChild(row);

    let colAddressType = document.createElement("DIV");
    let colAddress = document.createElement("DIV");
    let colCountry = document.createElement("DIV");
    let colCity = document.createElement("DIV");
    let colZip = document.createElement("DIV");
    let colPoBox = document.createElement("DIV");
    let hr = document.createElement("HR");
    let colPreferred = document.createElement("DIV");
    let colDelete = document.createElement("DIV");

    colAddressType.classList.add("col-sm-12", "col-md-3", "mb-3");
    colAddress.classList.add("col-sm-12", "col-md-6", "mb-3");
    colCountry.classList.add("col-sm-6", "col-md-3", "mb-3");
    colCity.classList.add("col-sm-6", "col-md-3", "mb-3");
    colZip.classList.add("col-sm-12", "col-md-3", "mb-3");
    colPoBox.classList.add("col-sm-12", "col-md-3", "mb-3");
    colPreferred.classList.add("col-sm-12", "col-md-2", "mb-3");
    colDelete.classList.add("col-sm-12", "col-md-1", "mb-3", "d-flex", "justify-content-center", "align-items-end");

    colAddressType.innerHTML = `
    <label class="form-label" for="basic-icon-default-fullname" name="">Address Of</label>
    <div class="input-group input-group-merge">
        <span id="basic-icon-default-fullname2" class="input-group-text"><i class="bx bx-building"></i></span>
        <select class="form-select" name="AddressTypeId[]">
            <option>Select Address...</option>
            @foreach (CompanyAddressType ca in ViewBag.AddressTypes)
            {
                <option value="@ca.CompanyAddressTypeId">@ca.AddressType</option>
            }
        </select>
    </div>
    `;

    colAddress.innerHTML = `
    <label class="form-label" for="basic-icon-default-fullname" name="">Address</label>
    <div class="input-group input-group-merge">
        <span id="basic-icon-default-fullname2" class="input-group-text"><i class="bx bx-building"></i></span>
        <input type="text" name="Address[]" class="form-control" placeholder="Enter Address">
    </div>
    <span class="text-danger"></span>
    `;

    colCountry.innerHTML = `
    <label class="form-label" for="basic-icon-default-fullname" name="">Country</label>
    <div class="input-group input-group-merge">
        <span id="basic-icon-default-fullname2" class="input-group-text"><i class="bx bx-building"></i></span>
        <input type="text" name="Country[]" class="form-control" placeholder="Enter Country">
    </div>
    <span class="text-danger"></span>
    `;

    colCity.innerHTML = `
    <label class="form-label" for="basic-icon-default-fullname" name="">City</label>
    <div class="input-group input-group-merge">
        <span id="basic-icon-default-fullname2" class="input-group-text"><i class="bx bx-building"></i></span>
        <input type="text" name="City[]" class="form-control" placeholder="Enter City">
    </div>
    <span class="text-danger"></span>
    `;

    colPoBox.innerHTML = `
    <label class="form-label" for="basic-icon-default-fullname" name="">Po Box</label>
    <div class="input-group input-group-merge">
        <span id="basic-icon-default-fullname2" class="input-group-text"><i class="bx bx-building"></i></span>
        <input type="text" name="PoBox[]" class="form-control" placeholder="Enter City">
    </div>
    <span class="text-danger"></span>
    `;


    colZip.innerHTML = `
    <label class="form-label" for="basic-icon-default-fullname" name="">Zip Code</label>
    <div class="input-group input-group-merge">
        <span id="basic-icon-default-fullname2" class="input-group-text"><i class="bx bx-building"></i></span>
        <input type="text" name="Zip[]" class="form-control" placeholder="Enter City">
    </div>
    <span class="text-danger"></span>
    `;

    colPreferred.innerHTML = `
    <label class="form-label" for="basic-icon-default-fullname" name="">Preferred</label>
    <div class="input-group input-group-merge">
        <span id="basic-icon-default-fullname2" class="input-group-text"><i class="bx bx-check"></i></span>
        <select class="form-select" name="AddressPreferred[]">
            <option value="true">Yes</option>
            <option value="false" selected>No</option>
        </select>
    </div>`;

    colDelete.innerHTML = `
    <button type="button" class="btn btn-icon btn-danger" onclick="removeAddress(${id})"><i class="bx bx-trash"></i></button>
    `;

    row.appendChild(colAddressType);
    row.appendChild(colAddress);
    row.appendChild(colCountry);
    row.appendChild(colCity);
    row.appendChild(colPoBox);
    row.appendChild(colZip);
    row.appendChild(colPreferred);
    row.appendChild(colDelete);
    row.appendChild(hr);

}
function removeAddress(id) {
    document.getElementById(`address_${id}`).remove();
}

function addContact() {

    let id = Date.now()
    let rootNode = document.getElementById("contacts");

    let row = document.createElement("DIV");
    row.classList.add("row");
    row.setAttribute("id", "contact_" + id);

    rootNode.appendChild(row);

    let colContactType = document.createElement("DIV");
    let colContact = document.createElement("DIV");
    let colDelete = document.createElement("DIV");
    let colPreferred = document.createElement("DIV");
    let hr = document.createElement("HR");

    colContactType.classList.add("col-sm-12", "col-md-3", "mb-3");
    colContact.classList.add("col-sm-12", "col-md-6", "mb-3");
    colDelete.classList.add("col-sm-12", "col-md-1", "mb-3", "d-flex", "justify-content-center", "align-items-end");
    colPreferred.classList.add("col-sm-12", "col-md-2", "mb-3");

    colContactType.innerHTML = `
    <label class="form-label" for="basic-icon-default-fullname" name="">Contact Type</label>
    <div class="input-group input-group-merge">
        <span id="basic-icon-default-fullname2" class="input-group-text"><i class="bx bx-phone"></i></span>
        <select class="form-select" name="ContactTypeId[]">
            <option>Select Contact Type...</option>
            @foreach (CompanyContactType ca in ViewBag.ContactTypes)
            {
                <option value="@ca.CompanyContactTypeId">@ca.ContactType</option>
            }
        </select>
    </div>
    `;

    colContact.innerHTML = `
    <label class="form-label" for="basic-icon-default-fullname" name="">Contact</label>
    <div class="input-group input-group-merge">
        <span id="basic-icon-default-fullname2" class="input-group-text"><i class="bx bx-phone"></i></span>
        <input type="text" name="Contact[]" class="form-control" placeholder="Enter Address" />
    </div>
    <span class="text-danger"></span>`;

    colPreferred.innerHTML = `
    <label class="form-label" for="basic-icon-default-fullname" name="">Preferred</label>
    <div class="input-group input-group-merge">
        <span id="basic-icon-default-fullname2" class="input-group-text"><i class="bx bx-check"></i></span>
        <select class="form-select" name="ContactPreferred[]">
            <option value="true">Yes</option>
            <option value="false" selected>No</option>
        </select>
    </div>
    `;


    colDelete.innerHTML = `
    <button type="button" class="btn btn-icon btn-danger" onclick="removeContact(${id})"><i class="bx bx-trash"></i></button>
    `;

    row.appendChild(colContactType);
    row.appendChild(colContact);
    row.appendChild(colPreferred);
    row.appendChild(colDelete);
    row.appendChild(hr);

}
function removeContact(id) {
    document.getElementById("contact_" + id).remove();
}

function addEmail() {
    let rootNode = document.getElementById("emails");
    let id = Date.now()
    let row = document.createElement("DIV");
    row.classList.add("row");
    row.setAttribute("id", "email_" + id);

    rootNode.appendChild(row);


    let colEmailType = document.createElement("DIV");
    let colEmail = document.createElement("DIV");

    let colPreferred = document.createElement("DIV");
    let colDelete = document.createElement("DIV");
    let hr = document.createElement("HR");

    colEmailType.classList.add("col-sm-12", "col-md-3", "mb-3");
    colEmail.classList.add("col-sm-12", "col-md-6", "mb-3");
    colDelete.classList.add("col-sm-12", "col-md-1", "mb-3", "d-flex", "justify-content-center", "align-items-end");
    colPreferred.classList.add("col-sm-12", "col-md-2", "mb-3");

    colEmailType.innerHTML = `
    <label class="form-label" for="basic-icon-default-fullname" name="">Email Type</label>
    <div class="input-group input-group-merge">
        <span id="basic-icon-default-fullname2"
            class="input-group-text"><i class="bx bx-envelope"></i></span>
        <select class="form-select" name="EmailTypeId[]">
            <option>Select Email Type...</option>
            @foreach (CompanyEmailType ca in ViewBag.EmailTypes)
            {
                <option value="@ca.CompanyEmailTypeId">@ca.EmailType</option>
            }
        </select>
    </div>
    <span class="text-danger"></span>
    `;

    colEmail.innerHTML = `
    <label class="form-label" for="basic-icon-default-fullname" name="">Email</label>
    <div class="input-group input-group-merge">
        <span id="basic-icon-default-fullname2" class="input-group-text"><i class="bx bx-envelope"></i></span>
        <input type="text" name="Email[]" class="form-control" placeholder="Enter Email Address" />
    </div>
    <span class="text-danger"></span>
    `;

    colPreferred.innerHTML = `
    <label class="form-label" for="basic-icon-default-fullname" name="">Preferred</label>
    <div class="input-group input-group-merge">
        <span id="basic-icon-default-fullname2" class="input-group-text"><i class="bx bx-check"></i></span>
        <select class="form-select" name="EmailPreferred[]">
            <option value="true">Yes</option>
            <option value="false" selected>No</option>
        </select>
    </div>
    `;


    colDelete.innerHTML = `
    <button type="button" class="btn btn-icon btn-danger" onclick="removeEmail(${id})"><i class="bx bx-trash"></i></button>
    `;

    row.appendChild(colEmailType);
    row.appendChild(colEmail);
    row.appendChild(colPreferred);
    row.appendChild(colDelete);
    row.appendChild(hr);
}
function removeEmail(id) {
    document.getElementById("email_" + id).remove();
}

function addLicense() {
    let rootNode = document.getElementById("licenses");
    let id = Date.now() + Math.floor(Math.random*10000)

    let row = document.createElement("DIV");
    row.classList.add("row");
    row.setAttribute("id", "license_" + id);

    rootNode.appendChild(row);

    let colLicenseType = document.createElement("DIV");
    let colLicense = document.createElement("DIV");
    let colIssuePlace = document.createElement("DIV");
    let colIssueDate = document.createElement("DIV");
    let colValid = document.createElement("DIV");
    let colPreferred = document.createElement("DIV");
    let colDelete = document.createElement("DIV");
    let hr = document.createElement("HR");


    colLicenseType.classList.add("col-sm-12", "col-md-4", "mb-3");
    colLicense.classList.add("col-sm-12", "col-md-8", "mb-3");
    colIssuePlace.classList.add("col-sm-12", "col-md-3", "mb-3");
    colIssueDate.classList.add("col-sm-12", "col-md-3", "mb-3");
    colValid.classList.add("col-sm-12", "col-md-3", "mb-3");
    colDelete.classList.add("col-sm-12", "col-md-1", "mb-3", "d-flex", "justify-content-center", "align-items-end");
    colPreferred.classList.add("col-sm-12", "col-md-2", "mb-3");

    colLicenseType.innerHTML = `
    <label class="form-label" for="basic-icon-default-fullname" name="">License Type</label>
    <div class="input-group input-group-merge">
        <span id="basic-icon-default-fullname2"
            class="input-group-text"><i class="bx bx-envelope"></i></span>
        <select class="form-select" name="LicenseTypeId[]">
            <option>Select License Type...</option>
            @foreach (CompanyLicenseType ca in ViewBag.LicenseTypes)
            {
                <option value="@ca.CompanyLicenseTypeId">@ca.LicenseName</option>
            }
        </select>
    </div>
    `;

    colLicense.innerHTML = `
    <label class="form-label" for="basic-icon-default-fullname" name="">License #</label>
    <div class="input-group input-group-merge">
        <span id="basic-icon-default-fullname2" class="input-group-text"><i class="bx bx-envelope"></i></span>
        <input type="text" name="LicenseNo[]" class="form-control" placeholder="Enter Email Address" />
    </div>
    <span class="text-danger"></span>
    `;

    colIssuePlace.innerHTML = `
    <label class="form-label" for="basic-icon-default-fullname" name="">Place of Issue</label>
    <div class="input-group input-group-merge">
        <span id="basic-icon-default-fullname2" class="input-group-text"><i class="bx bx-envelope"></i></span>
        <input type="text" name="IssuePlace[]" class="form-control" placeholder="Enter Email Address" />
    </div>
    <span class="text-danger"></span>
    `;

    colIssueDate.innerHTML = `
    <label class="form-label" for="basic-icon-default-fullname" name="">Date of Issue</label>
    <div class="input-group input-group-merge">
        <span id="basic-icon-default-fullname2" class="input-group-text"><i class="bx bx-envelope"></i></span>
        <input type="date" name="IssueDate[]" class="form-control" placeholder="Enter Email Address" />
    </div>
    <span class="text-danger"></span>

    `;

    colValid.innerHTML = `
    <label class="form-label" for="basic-icon-default-fullname" name="">Valid Upto</label>
    <div class="input-group input-group-merge">
        <span id="basic-icon-default-fullname2" class="input-group-text"><i class="bx bx-envelope"></i></span>
        <input type="date" name="ValidDate[]" class="form-control" placeholder="Enter Email Address" />
    </div>
    <span class="text-danger"></span>
    `;

    colPreferred.innerHTML = `
    <label class="form-label" for="basic-icon-default-fullname" name="">Preferred</label>
    <div class="input-group input-group-merge">
        <span id="basic-icon-default-fullname2" class="input-group-text"><i class="bx bx-check"></i></span>
        <select class="form-select" name="LicensePreferred[]">
            <option value="true">Yes</option>
            <option value="false" selected>No</option>
        </select>
    </div>
    `;


    colDelete.innerHTML = `
    <button type="button" class="btn btn-icon btn-danger" onclick="removeLicense(${id})"><i class="bx bx-trash"></i></button>
    `;


    row.appendChild(colLicenseType);
    row.appendChild(colLicense);
    row.appendChild(colIssuePlace);
    row.appendChild(colIssueDate);
    row.appendChild(colValid);
    row.appendChild(colPreferred);
    row.appendChild(colDelete);
    row.appendChild(hr);
}

function removeLicense(id) {
    document.getElementById("license_" + id).remove();

}


