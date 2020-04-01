<?xml version="1.0" encoding="ISO-8859-1"?>

<xsl:stylesheet
xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
xmlns="http://www.w3.org/TR/REC-html40"
version="2.0">

  <xsl:output method="html"/>

  <!-- Root template -->
  <xsl:template match="/">
    <html>
      <head>
        <title>
          <xsl:value-of select="/root/display_name" />
        </title>
        <link rel="stylesheet" type="text/css" href="../css/bpdoc.css" />
      </head>
      <body>
        <div id="content_container">
          <xsl:apply-templates select="/root" />
        </div>
      </body>
    </html>
  </xsl:template>

  <xsl:template match="/root">
    <a class="navbar_style">
      <xsl:attribute name="href">../index.html</xsl:attribute>
      <xsl:value-of select="docs_name" />
    </a>
    <a class="navbar_style">&gt;</a>
    <a class="navbar_style">
      <xsl:value-of select="display_name" />
    </a>


    <div class="hero">
      <div class="info">
        <div id="pageTitle">
          <h1 id="H1TitleId">
            <xsl:value-of select="display_name" />
          </h1>
        </div>
        <h2>
          Todo here is the function header explanation
          <xsl:value-of select="description" />
        </h2>
      </div>
    </div>  
    <xsl:apply-templates select="FunctionList" />
  </xsl:template>


  <!--Templates to match specific elements in the input xml-->
  <xsl:template match="FunctionList">
    <h2 class="title">Variables</h2>
    <div class="member-list">
      <table cellspacing="0">
        <tbody>
          <tr class="header-row">
            <th class="icon-cell" style="width:8%"></th>
            <th class="type-cell" style="width:10%"></th>
            <th class="name-cell" style="width:15%">Name</th>
            <th class="desc-cell" style="width:8%">Description</th>
          </tr>
          <xsl:for-each select="Function">
            <xsl:if test="node_type='Variable'">
              <!--<xsl:apply-templates select="Function">-->

              <tr class="normal-row">
                <td class="icon-cell">
                  <xsl:if test="access_spec='Public'">
                    <img src="../../../DeploymentFiles/img/api_variable_public.png" alt="Public"/>
                  </xsl:if>
                  <xsl:if test="access_spec='Protected'">
                    <img src="../../../DeploymentFiles/img/api_variable_protected.png" alt="Protected"/>
                  </xsl:if>
                  <xsl:if test="access_spec='Private'">
                    <img src="../../../DeploymentFiles/img/api_variable_private.png" alt="Private"/>
                  </xsl:if>
                </td>
                <td class="name-cell" align="right">
                  <span class="type-span">
                    <p>
                      <xsl:value-of select="outputs/param/type"/>
                    </p>
                  </span>
                </td>
                <td class="name-cell">
                  <p>
                    <xsl:value-of select="shorttitle"/>
                  </p>
                </td>
                <td class="desc-cell">
                  <xsl:value-of select="description"/>
                </td>
              </tr>
            </xsl:if>
          </xsl:for-each>
        </tbody>
      </table>

    </div>

    <h2 class="title_style">Functions</h2>


    <xsl:for-each select="Function">
      <xsl:if test="node_type='Function'">
        <table>
          <tr>
            <td colspan="2" align="left">
              <xsl:value-of select="shorttitle"/>
            </td>
          </tr>
          <tr >
            <td colspan="2" align="left">
              <img src="{imgpath}" align="center"/>
            </td>
          </tr>
          <tr>
            <td>
              Editability:
            </td>
            <td>
              <xsl:value-of select="edit_type"/>
            </td>
          </tr>
          <tr>
            <td>
              Accesibility:
            </td>
            <td>
              <xsl:value-of select="access_spec"/>
            </td>
          </tr>
          <tr>
            <td>
              Description:
            </td>
            <td>
              <xsl:value-of select="description"/>
            </td>
          </tr>
          <tr>
            <td>
              Inputs:
            </td>
            <td>
              <table>
                <xsl:for-each select="inputs/param">
                  <xsl:if test="type!='Exec'">
                    <tr>
                      <td>name</td>
                      <td>
                        <xsl:value-of select="name"/>
                      </td>
                    </tr>
                    <tr>
                      <td>type</td>
                      <td>
                        <xsl:value-of select="type"/>
                      </td>
                    </tr>
                    <tr>
                      <td>description</td>
                      <td>
                        <xsl:value-of select="description"/>
                      </td>
                    </tr>
                  </xsl:if>
                </xsl:for-each>
              </table>
            </td>
          </tr>
          <tr>
            <td>
              Outputs:
            </td>
            <td>
              <table>
                <xsl:for-each select="outputs/param">
                  <xsl:if test="type!='Exec'">
                    <tr>
                      <td>name</td>
                      <td>
                        <xsl:value-of select="name"/>
                      </td>
                    </tr>
                    <tr>
                      <td>type</td>
                      <td>
                        <xsl:value-of select="type"/>
                      </td>
                    </tr>
                    <tr>
                      <td>description</td>
                      <td>
                        <xsl:value-of select="description"/>
                      </td>
                    </tr>
                  </xsl:if>
                </xsl:for-each>
              </table>
            </td>
          </tr>
        </table>
      </xsl:if>
    </xsl:for-each>


  </xsl:template>

</xsl:stylesheet>
