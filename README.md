# Elite Tracker
Elite Tracker is a group community tool for Elite Dangerous. It is built on ASP.NET MVC and powered by <a href="http://www.ravendb.net">RavenDB</a>. To run this you'll need a RavenDB license. Please contact them for an open source license.

<h2>Features</h2>
<h5>Solar systems</h5>
<ul>
<li>Track influence over time. Graphs included</li>
<li>Add coordinates to list other systems by distance</li>
<li>Comments</li>
<li>Add market data. Can be imported from Cmdr's log</li>
</ul>

<h5>Tasks</h5>
Create tasks that commanders can sign up for.
<ul>
<li>Create tasks like "Recon expansion for LHS 3447"</li>
<li>Prioritized</li>
<li>Commanders can sign up for tasks</li>
<li>Comments</li>
</ul>

<h5>Commanders</h5>
<ul>
<li>Profile page</li>
<li>Ships</li>
</ul>

<h5>Resources</h5>
Common articles that anyone can edit.
<ul>
<li>Tracks changes</li>
<li>Comments</li>
</ul>

<h4>Requirements</h4>
Needs to be hosted on a windows machine running IIS and ASP.NET 4.

<h4>Installation</h4>
Compile project. Visual studio prefered. Setup website in IIS and copy over the necessary files. In web.config change app setting "installation" to "1". Open web site in a browser. As long as installation is set to "1" and there are no users the first login will create a user.

<h4>Restore data from backup</h4>
If you need to restore data from an existing installation go ahead with the "Installation" above. But instead of logging in you first need to set the app setting "jobkey" to a password like string. Then call http://yoursite/data/restore?key=jobkey&path=relative_path_to_backup_file

<h4>Backup</h4>
Backups can be made by calling http://yoursite/job/backup/?key=jobkey where jobkey is set in web.config. Backup will be created at the location indicated in the web.config app setting "BackupPath". Existing backup will be overwritten.
